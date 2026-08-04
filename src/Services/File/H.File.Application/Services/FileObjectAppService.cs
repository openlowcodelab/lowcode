using System.Text.RegularExpressions;
using H.File.Application.Contracts;
using H.File.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.File.Application.Services;

/// <summary>
/// 文件对象应用服务（操作 MinIO 中的文件）
/// </summary>
public class FileObjectAppService : ApplicationService, IFileObjectAppService
{
    private readonly IRepository<FileProjectEntity, Guid> _repository;
    private readonly IRepository<FileFolderEntity, Guid> _folderRepository;
    private readonly IRepository<FileObjectEntity, Guid> _objectRepository;
    private readonly MinioStorageService _storage;

    public FileObjectAppService(
        IRepository<FileProjectEntity, Guid> repository,
        IRepository<FileFolderEntity, Guid> folderRepository,
        IRepository<FileObjectEntity, Guid> objectRepository,
        MinioStorageService storage)
    {
        _repository = repository;
        _folderRepository = folderRepository;
        _objectRepository = objectRepository;
        _storage = storage;
    }

    public async Task<List<FileObjectDto>> GetFilesAsync(Guid projectId, string? folderPath = null)
    {
        var prefix = string.IsNullOrEmpty(folderPath) ? string.Empty : EnsureTrailingSlash(folderPath);

        var queryable = await _objectRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(
            queryable.Where(o => o.ProjectId == projectId && o.FolderPath == prefix));

        return entities.Select(e => new FileObjectDto
        {
            Id = e.Id,
            FileName = e.FileName,
            Size = e.Size,
            ContentType = e.ContentType,
            LastModified = e.CreationTime,
            IsFolder = false
        }).OrderBy(x => x.FileName).ToList();
    }

    public async Task<List<FileFolderDto>> GetFolderTreeAsync(Guid projectId)
    {
        await _repository.GetAsync(projectId);
        var folders = await _folderRepository.GetListAsync(f => f.ProjectId == projectId);

        var nodes = new Dictionary<string, FileFolderDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in folders)
        {
            nodes[f.Path] = new FileFolderDto { Path = f.Path, Code = f.Code, Name = f.Name };
        }

        var roots = new List<FileFolderDto>();
        foreach (var f in folders.OrderBy(x => x.Name))
        {
            var node = nodes[f.Path];
            var parentPath = GetParentPath(f.Path, f.Code);
            if (parentPath.Length == 0)
                roots.Add(node);
            else if (nodes.TryGetValue(parentPath, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }

    public async Task<FileUploadResultDto> UploadAsync(Guid projectId, string? folderPath = null, string fileName = "", byte[]? content = null, string? contentType = null)
    {
        if (content is null)
            throw new UserFriendlyException("文件内容不能为空");
        var entity = await _repository.GetAsync(projectId);
        // 预生成实体主键作为 MinIO 对象名（避免中文等特殊字符引发的编码问题），并按 FolderPath 存入对应目录
        var objectId = GuidGenerator.Create();
        var folder = string.IsNullOrEmpty(folderPath) ? string.Empty : EnsureTrailingSlash(folderPath);
        var objectKey = folder + objectId.ToString("N");
        contentType ??= GetContentType(fileName);

        try
        {
            await _storage.PutObjectAsync(entity.BucketName, objectKey, content, contentType);

            // 写入文件元数据到数据库
            await _objectRepository.InsertAsync(new FileObjectEntity(objectId)
            {
                ProjectId = projectId,
                FileName = fileName,
                Size = content.Length,
                ContentType = contentType,
                FolderPath = folder
            });

            // 更新项目统计信息
            entity.FileCount += 1;
            entity.TotalSize += content.Length;
            await _repository.UpdateAsync(entity);

            return new FileUploadResultDto
            {
                Id = objectId,
                FileName = fileName,
                Size = content.Length,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new FileUploadResultDto
            {
                Id = objectId,
                FileName = fileName,
                Size = content.Length,
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<FileDownloadResultDto> GetDownloadUrlAsync(Guid projectId, Guid fileId)
    {
        // 通过服务端代理下载，避免预签名 URL 对中文等特殊字符对象名处理异常
        return new FileDownloadResultDto { Url = $"/api/file/download?projectId={projectId}&fileId={fileId}" };
    }

    public async Task<FilePreviewDto> GetPreviewAsync(Guid projectId, Guid fileId)
    {
        var entity = await _repository.GetAsync(projectId);
        // 对象名为 FolderPath + 实体主键，显示文件名/类型从数据库元数据获取
        var fileObject = await _objectRepository.FindAsync(o => o.Id == fileId && o.ProjectId == projectId);
        if (fileObject == null)
            throw new UserFriendlyException("文件不存在");
        var objectKey = GetObjectKey(fileObject);
        var fileName = fileObject.FileName;
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = fileObject.ContentType;

        var preview = new FilePreviewDto
        {
            FileName = fileName,
            ContentType = contentType,
            PreviewType = GetPreviewType(ext)
        };

        // 文本类文件直接读取内容
        if (preview.PreviewType is "text" or "markdown" or "html")
        {
            var data = await _storage.GetObjectAsync(entity.BucketName, objectKey);
            preview.TextContent = System.Text.Encoding.UTF8.GetString(data);
            preview.Size = data.Length;
        }
        else if (preview.PreviewType is "image" or "pdf")
        {
            // 图片和PDF通过服务端代理内联预览（避免预签名 URL 对中文等特殊字符对象名处理异常）
            preview.PreviewUrl = $"/api/file/preview?projectId={projectId}&fileId={fileId}";
        }
        else if (preview.PreviewType == "office")
        {
            // Office 文件：前端使用 docx-preview / xlsx / pptx-preview 等库解析 Base64 内容渲染预览
            var size = await _storage.GetObjectSizeAsync(entity.BucketName, objectKey);
            preview.Size = size ?? 0;
            if (size.HasValue && size.Value <= MaxOfficePreviewSize)
            {
                var data = await _storage.GetObjectAsync(entity.BucketName, objectKey);
                preview.Base64Content = Convert.ToBase64String(data);
            }
            else
            {
                // 文件过大时提供下载查看入口
                preview.PreviewUrl = $"/api/file/download?projectId={projectId}&fileId={fileId}";
            }
        }

        return preview;
    }

    public async Task DeleteAsync(Guid projectId, Guid fileId)
    {
        var entity = await _repository.GetAsync(projectId);

        var fileEntity = await _objectRepository.FindAsync(o => o.Id == fileId && o.ProjectId == projectId);
        if (fileEntity == null)
            throw new UserFriendlyException("文件不存在");

        var objectKey = GetObjectKey(fileEntity);
        await _storage.RemoveObjectAsync(entity.BucketName, objectKey);
        await _objectRepository.DeleteAsync(fileEntity);

        // 更新项目统计信息
        entity.FileCount -= 1;
        entity.TotalSize -= fileEntity.Size;
        await _repository.UpdateAsync(entity);
    }

    public async Task CreateFolderAsync(Guid projectId, CreateFolderInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
            throw new UserFriendlyException("分类名称不能为空");
        if (input.Name.Contains('/'))
            throw new UserFriendlyException("分类名称不能包含 / 字符");

        var entity = await _repository.GetAsync(projectId);
        var parentPath = NormalizeParentPath(input.ParentPath);
        if (GetFolderDepth(parentPath) >= MaxFolderDepth)
            throw new UserFriendlyException($"分类层级最多只能 {MaxFolderDepth} 层");

        var code = string.IsNullOrWhiteSpace(input.Code)
            ? await GenerateUniqueFolderCodeAsync(projectId, parentPath)
            : input.Code.Trim().ToLowerInvariant();

        if (!FolderCodeRegex.IsMatch(code))
            throw new UserFriendlyException("分类编号需为 3-20 位小写字母");

        var path = parentPath + code + "/";
        if (await _folderRepository.AnyAsync(f => f.ProjectId == projectId && f.Path == path))
            throw new UserFriendlyException($"分类编号「{code}」在该层级已存在");

        await _storage.PutObjectAsync(entity.BucketName, path, Array.Empty<byte>(), "application/x-directory");

        await _folderRepository.InsertAsync(new FileFolderEntity
        {
            ProjectId = projectId,
            Code = code,
            Name = input.Name.Trim(),
            Path = path
        });
    }

    public async Task RenameFolderAsync(Guid projectId, string folderPath, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new UserFriendlyException("分类名称不能为空");
        if (newName.Contains('/'))
            throw new UserFriendlyException("分类名称不能包含 / 字符");

        var folder = await _folderRepository.FirstOrDefaultAsync(f =>
            f.ProjectId == projectId && f.Path == EnsureTrailingSlash(folderPath));
        if (folder == null)
            throw new UserFriendlyException("分类不存在");

        folder.Name = newName.Trim();
        await _folderRepository.UpdateAsync(folder);
    }

    public async Task DeleteFolderAsync(Guid projectId, string folderPath)
    {
        var entity = await _repository.GetAsync(projectId);
        var prefix = EnsureTrailingSlash(folderPath);
        await _storage.RemoveObjectsByPrefixAsync(entity.BucketName, prefix);

        // 从数据库删除该文件夹下的所有文件元数据
        var queryable = await _objectRepository.GetQueryableAsync();
        var toDelete = await AsyncExecuter.ToListAsync(
            queryable.Where(o => o.ProjectId == projectId && o.FolderPath.StartsWith(prefix)));
        if (toDelete.Any())
        {
            // 更新项目统计信息
            entity.FileCount -= toDelete.Count;
            entity.TotalSize -= toDelete.Sum(o => o.Size);
            await _repository.UpdateAsync(entity);

            await _objectRepository.DeleteManyAsync(toDelete);
        }

        // 删除文件夹记录
        var toDeleteFolders = await _folderRepository.GetListAsync(f =>
            f.ProjectId == projectId && (f.Path == prefix || f.Path.StartsWith(prefix)));
        await _folderRepository.DeleteManyAsync(toDeleteFolders);
    }

    public async Task<MultipartUploadDto> InitMultipartUploadAsync(Guid projectId, string? folderPath = null, string fileName = "", long fileSize = 0, string? contentType = null)
    {
        var entity = await _repository.GetAsync(projectId);
        // 预生成文件实体主键作为 MinIO 对象名，按 FolderPath 存入对应目录（合并完成时以此主键写入数据库）
        var objectId = GuidGenerator.Create();
        var folder = string.IsNullOrEmpty(folderPath) ? string.Empty : EnsureTrailingSlash(folderPath);
        var objectKey = folder + objectId.ToString("N");
        // 使用唯一 ID 作为临时目录前缀
        var uploadId = Guid.NewGuid().ToString("N");
        var totalParts = (int)Math.Ceiling((double)fileSize / MultipartUploadConstants.DefaultPartSize);

        return new MultipartUploadDto
        {
            UploadId = uploadId,
            ObjectId = objectId,
            ObjectKey = objectKey,
            TotalParts = totalParts,
            PartSize = MultipartUploadConstants.DefaultPartSize
        };
    }

    public async Task<UploadPartResult> UploadPartAsync(UploadPartInput input)
    {
        try
        {
            // 临时分片存储路径：.multipart/{uploadId}/part-{index}
            var partKey = $".multipart/{input.UploadId}/part-{input.PartIndex}";
            // 从 UploadId 第一次调用时推断 bucket（通过 ObjectKey 查找项目）
            // 这里简化：将分片数据直接上传到第一个项目的 bucket
            // 实际应该在 InitMultipartUploadAsync 时保存 bucketName
            var queryable = await _objectRepository.GetQueryableAsync();
            // 分片上传时需要知道 bucketName，通过临时存储解决
            await _storage.PutObjectAsync(input.BucketName, partKey, input.Data);

            return new UploadPartResult
            {
                PartIndex = input.PartIndex,
                ETag = $"part-{input.PartIndex}",
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new UploadPartResult
            {
                PartIndex = input.PartIndex,
                Success = false,
                Message = ex.Message
            };
        }
    }

    public Task<List<UploadedPartDto>> ListUploadedPartsAsync(string uploadId, string objectKey)
    {
        return Task.FromResult(new List<UploadedPartDto>());
    }

    public async Task<FileUploadResultDto> CompleteMultipartUploadAsync(CompleteMultipartUploadInput input)
    {
        try
        {
            var entity = await _repository.GetAsync(input.ProjectId);
            var bucketName = entity.BucketName;

            // 读取所有分片并合并
            using var combinedStream = new MemoryStream();
            for (int i = 1; i <= input.Parts.Count; i++)
            {
                var partKey = $".multipart/{input.UploadId}/part-{i}";
                var partData = await _storage.GetObjectAsync(bucketName, partKey);
                await combinedStream.WriteAsync(partData);
                // 删除临时分片
                await _storage.RemoveObjectAsync(bucketName, partKey);
            }

            // 上传合并后的文件
            combinedStream.Position = 0;
            var contentType = input.ContentType ?? GetContentType(input.FileName);
            await _storage.PutObjectAsync(bucketName, input.ObjectKey, combinedStream.ToArray(), contentType);

            // 写入文件元数据到数据库（主键即 Init 阶段预生成的对象名）
            var fileId = input.ObjectId == Guid.Empty ? GuidGenerator.Create() : input.ObjectId;
            await _objectRepository.InsertAsync(new FileObjectEntity(fileId)
            {
                ProjectId = input.ProjectId,
                FileName = input.FileName,
                Size = input.FileSize,
                ContentType = contentType,
                FolderPath = string.IsNullOrEmpty(input.FolderPath) ? string.Empty : EnsureTrailingSlash(input.FolderPath)
            });

            // 更新项目统计信息
            entity.FileCount += 1;
            entity.TotalSize += input.FileSize;
            await _repository.UpdateAsync(entity);

            return new FileUploadResultDto
            {
                Id = fileId,
                FileName = input.FileName,
                Size = input.FileSize,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new FileUploadResultDto
            {
                FileName = input.FileName,
                Size = input.FileSize,
                Success = false,
                Message = ex.Message
            };
        }
    }

    public Task AbortMultipartUploadAsync(string uploadId, string objectKey)
    {
        return Task.CompletedTask;
    }

    #region 辅助方法

    /// <summary>Office 文件在线预览的最大大小（超过则提示下载查看）</summary>
    private const long MaxOfficePreviewSize = 30L * 1024 * 1024;

    /// <summary>文件分类最大层级数</summary>
    private const int MaxFolderDepth = 5;

    /// <summary>根据实体构造 MinIO 对象完整路径（FolderPath + 主键）</summary>
    private static string GetObjectKey(FileObjectEntity fileObject)
        => fileObject.FolderPath + fileObject.Id.ToString("N");

    /// <summary>计算分类路径的层级数（如 "abc/def/" 为 2 层）</summary>
    private static int GetFolderDepth(string path)
        => path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;

    private const string FolderCodeChars = "abcdefghijklmnopqrstuvwxyz";

    private static readonly Regex FolderCodeRegex = new("^[a-z]{3,20}$", RegexOptions.Compiled);

    private static string EnsureTrailingSlash(string path)
        => path.EndsWith('/') ? path : path + "/";

    private static string NormalizeParentPath(string? parentPath)
    {
        if (string.IsNullOrEmpty(parentPath))
            return string.Empty;
        return parentPath.EndsWith('/') ? parentPath : parentPath + "/";
    }

    private static string GetParentPath(string path, string code)
    {
        var segment = code + "/";
        return path.EndsWith(segment, StringComparison.OrdinalIgnoreCase)
            ? path[..^segment.Length]
            : string.Empty;
    }

    private static string GenerateRandomCode()
    {
        var chars = new char[8];
        for (int i = 0; i < chars.Length; i++)
            chars[i] = FolderCodeChars[Random.Shared.Next(FolderCodeChars.Length)];
        return new string(chars);
    }

    private async Task<string> GenerateUniqueFolderCodeAsync(Guid projectId, string parentPath)
    {
        for (int i = 0; i < 10; i++)
        {
            var code = GenerateRandomCode();
            if (!await _folderRepository.AnyAsync(f => f.ProjectId == projectId && f.Path == parentPath + code + "/"))
                return code;
        }
        throw new UserFriendlyException("无法生成唯一的分类编号，请重试");
    }

    private static string GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".txt" => "text/plain",
            ".md" => "text/markdown",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".mp4" => "video/mp4",
            ".mp3" => "audio/mpeg",
            _ => "application/octet-stream"
        };
    }

    private static string GetPreviewType(string ext) => ext switch
    {
        ".txt" or ".log" or ".csv" or ".json" or ".xml" or ".css" or ".js" => "text",
        ".md" => "markdown",
        ".html" or ".htm" => "html",
        ".png" or ".jpg" or ".jpeg" or ".gif" or ".svg" or ".webp" or ".bmp" => "image",
        ".pdf" => "pdf",
        ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" => "office",
        _ => "unsupported"
    };

    #endregion
}
