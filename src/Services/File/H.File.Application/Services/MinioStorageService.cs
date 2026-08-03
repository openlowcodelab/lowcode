using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace H.File.Application.Services;

/// <summary>
/// MinIO 存储服务（封装 MinIO SDK 操作）
/// </summary>
public class MinioStorageService
{
    private readonly IMinioClient _client;
    private readonly MinioOptions _options;

    public MinioStorageService(IMinioClient client, IOptions<MinioOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <summary>创建 Bucket</summary>
    public async Task CreateBucketAsync(string bucketName)
    {
        var found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found)
        {
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }
    }

    /// <summary>删除 Bucket（含所有对象）</summary>
    public async Task DeleteBucketAsync(string bucketName)
    {
        var found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!found) return;

        // 先删除所有对象
        var objects = await ListAllObjectsInternalAsync(bucketName);
        foreach (var item in objects)
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(item.Key));
        }

        await _client.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucketName));
    }

    /// <summary>列出指定前缀下的对象（非递归）</summary>
    public async Task<List<(string Key, long Size, DateTime LastModified)>> ListObjectsAsync(string bucketName, string? prefix = null)
    {
        var args = new ListObjectsArgs().WithBucket(bucketName).WithRecursive(false);
        if (!string.IsNullOrEmpty(prefix))
            args = args.WithPrefix(prefix);

        var result = new List<(string, long, DateTime)>();
        await foreach (var item in _client.ListObjectsEnumAsync(args))
        {
            result.Add((item.Key, (long)item.Size, item.LastModifiedDateTime ?? DateTime.MinValue));
        }
        return result;
    }

    /// <summary>递归列出所有对象</summary>
    public async Task<List<(string Key, long Size, DateTime LastModified)>> ListAllObjectsAsync(string bucketName)
    {
        var items = await ListAllObjectsInternalAsync(bucketName);
        return items.Select(i => (i.Key, (long)i.Size, i.LastModifiedDateTime ?? DateTime.MinValue)).ToList();
    }

    /// <summary>上传对象（SDK 自动处理分片：>5MB 自动使用 multipart upload）</summary>
    public async Task PutObjectAsync(string bucketName, string objectName, byte[] data, string? contentType = null, IProgress<Minio.DataModel.ProgressReport>? progress = null)
    {
        if (data.Length == 0)
            data = [0];
        using var stream = new MemoryStream(data);
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(data.Length);

        if (!string.IsNullOrEmpty(contentType))
            args = args.WithContentType(contentType);

        if (progress != null)
            args = args.WithProgress(progress);

        await _client.PutObjectAsync(args);
    }

    /// <summary>获取对象数据</summary>
    public async Task<byte[]> GetObjectAsync(string bucketName, string objectName)
    {
        using var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(async (stream, ct) =>
            {
                await stream.CopyToAsync(ms, ct);
            }));
        return ms.ToArray();
    }

    /// <summary>删除对象</summary>
    public async Task RemoveObjectAsync(string bucketName, string objectName)
    {
        await _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName));
    }

    /// <summary>复制对象（服务端复制）</summary>
    public async Task CopyObjectAsync(string bucketName, string sourceObjectName, string targetObjectName)
    {
        var args = new CopyObjectArgs()
            .WithBucket(bucketName)
            .WithObject(targetObjectName)
            .WithCopyObjectSource(new CopySourceObjectArgs()
                .WithBucket(bucketName)
                .WithObject(sourceObjectName));
        await _client.CopyObjectAsync(args);
    }

    /// <summary>删除多个对象（按前缀）</summary>
    public async Task RemoveObjectsByPrefixAsync(string bucketName, string prefix)
    {
        var args = new ListObjectsArgs().WithBucket(bucketName).WithPrefix(prefix).WithRecursive(true);
        await foreach (var item in _client.ListObjectsEnumAsync(args))
        {
            await _client.RemoveObjectAsync(new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(item.Key));
        }
    }

    /// <summary>生成预签名下载URL</summary>
    public async Task<string> GetPresignedDownloadUrlAsync(string bucketName, string objectName, int expiryMinutes = 60)
    {
        return await _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryMinutes * 60));
    }

    /// <summary>获取对象元信息（ContentType）</summary>
    public async Task<string?> GetObjectContentTypeAsync(string bucketName, string objectName)
    {
        try
        {
            var stat = await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName));
            return stat.ContentType;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>获取对象大小（字节）</summary>
    public async Task<long?> GetObjectSizeAsync(string bucketName, string objectName)
    {
        try
        {
            var stat = await _client.StatObjectAsync(new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName));
            return (long)stat.Size;
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<Item>> ListAllObjectsInternalAsync(string bucketName)
    {
        var args = new ListObjectsArgs().WithBucket(bucketName).WithRecursive(true);
        var result = new List<Item>();
        await foreach (var item in _client.ListObjectsEnumAsync(args))
        {
            result.Add(item);
        }
        return result;
    }
}
