using H.File.Application.Contracts;
using H.File.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.File.Application.Services;

/// <summary>
/// 文件对象应用服务（操作 MinIO 中的文件）
/// </summary>
public class FileObjectAppService : ApplicationService, IFileObjectAppService
{
    private readonly IRepository<FileProjectEntity, Guid> _repository;
    private readonly MinioStorageService _storage;

    public FileObjectAppService(
        IRepository<FileProjectEntity, Guid> repository,
        MinioStorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<List<FileObjectDto>> GetFilesAsync(Guid projectId, string? folderPath = null)
    {
        var entity = await _repository.GetAsync(projectId);
        var prefix = string.IsNullOrEmpty(folderPath) ? null : EnsureTrailingSlash(folderPath);
        var objects = await _storage.ListObjectsAsync(entity.BucketName, prefix);

        var result = new List<FileObjectDto>();
        foreach (var (key, size, lastModified) in objects)
        {
            var isFolder = key.EndsWith('/');
            var fileName = GetFileName(key);
            result.Add(new FileObjectDto
            {
                Key = key,
                FileName = fileName,
                Size = size,
                LastModified = lastModified,
                IsFolder = isFolder,
                ContentType = isFolder ? null : GetContentType(fileName)
            });
        }

        // 文件夹在前，文件在后
        return result.OrderByDescending(x => x.IsFolder).ThenBy(x => x.FileName).ToList();
    }

    public async Task<List<FileFolderDto>> GetFolderTreeAsync(Guid projectId)
    {
        var entity = await _repository.GetAsync(projectId);
        var objects = await _storage.ListAllObjectsAsync(entity.BucketName);

        var root = new List<FileFolderDto>();
        var folderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, _, _) in objects)
        {
            var parts = key.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                var path = string.Join('/', parts[..(i + 1)]) + "/";
                folderSet.Add(path);
            }
        }

        // 构建树
        foreach (var path in folderSet.OrderBy(p => p))
        {
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var name = parts[^1];
            var parentPath = parts.Length > 1 ? string.Join('/', parts[..^1]) + "/" : "";

            var node = new FileFolderDto { Path = path, Name = name };

            if (string.IsNullOrEmpty(parentPath))
            {
                root.Add(node);
            }
            else
            {
                var parent = FindNode(root, parentPath);
                parent?.Children.Add(node);
            }
        }

        return root;
    }

    public async Task<FileUploadResultDto> UploadAsync(Guid projectId, string folderPath, string fileName, byte[] content, string? contentType = null)
    {
        var entity = await _repository.GetAsync(projectId);
        var objectKey = string.IsNullOrEmpty(folderPath) ? fileName : $"{EnsureTrailingSlash(folderPath)}{fileName}";
        contentType ??= GetContentType(fileName);

        try
        {
            await _storage.PutObjectAsync(entity.BucketName, objectKey, content, contentType);
            return new FileUploadResultDto
            {
                Key = objectKey,
                FileName = fileName,
                Size = content.Length,
                Success = true
            };
        }
        catch (Exception ex)
        {
            return new FileUploadResultDto
            {
                Key = objectKey,
                FileName = fileName,
                Size = content.Length,
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<string> GetDownloadUrlAsync(Guid projectId, string fileKey)
    {
        var entity = await _repository.GetAsync(projectId);
        return await _storage.GetPresignedDownloadUrlAsync(entity.BucketName, fileKey);
    }

    public async Task<FilePreviewDto> GetPreviewAsync(Guid projectId, string fileKey)
    {
        var entity = await _repository.GetAsync(projectId);
        var fileName = GetFileName(fileKey);
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = await _storage.GetObjectContentTypeAsync(entity.BucketName, fileKey) ?? GetContentType(fileName);

        var preview = new FilePreviewDto
        {
            FileName = fileName,
            ContentType = contentType,
            PreviewType = GetPreviewType(ext)
        };

        // 文本类文件直接读取内容
        if (preview.PreviewType is "text" or "markdown" or "html")
        {
            var data = await _storage.GetObjectAsync(entity.BucketName, fileKey);
            preview.TextContent = System.Text.Encoding.UTF8.GetString(data);
            preview.Size = data.Length;
        }
        else if (preview.PreviewType is "image" or "pdf")
        {
            // 图片和PDF使用预签名URL预览
            preview.PreviewUrl = await _storage.GetPresignedDownloadUrlAsync(entity.BucketName, fileKey, 30);
        }
        else if (preview.PreviewType == "office")
        {
            // Office 文件提供下载URL，前端使用第三方预览
            preview.PreviewUrl = await _storage.GetPresignedDownloadUrlAsync(entity.BucketName, fileKey, 30);
        }

        return preview;
    }

    public async Task DeleteAsync(Guid projectId, string fileKey)
    {
        var entity = await _repository.GetAsync(projectId);
        await _storage.RemoveObjectAsync(entity.BucketName, fileKey);
    }

    public async Task CreateFolderAsync(Guid projectId, string folderPath)
    {
        var entity = await _repository.GetAsync(projectId);
        var objectKey = EnsureTrailingSlash(folderPath);
        // MinIO 中文件夹通过空对象 + 尾部斜杠表示
        await _storage.PutObjectAsync(entity.BucketName, objectKey, Array.Empty<byte>(), "application/x-directory");
    }

    public async Task DeleteFolderAsync(Guid projectId, string folderPath)
    {
        var entity = await _repository.GetAsync(projectId);
        var prefix = EnsureTrailingSlash(folderPath);
        await _storage.RemoveObjectsByPrefixAsync(entity.BucketName, prefix);
    }

    #region 辅助方法

    private static string EnsureTrailingSlash(string path)
        => path.EndsWith('/') ? path : path + "/";

    private static string GetFileName(string key)
    {
        var trimmed = key.TrimEnd('/');
        var idx = trimmed.LastIndexOf('/');
        return idx >= 0 ? trimmed[(idx + 1)..] : trimmed;
    }

    private static FileFolderDto? FindNode(List<FileFolderDto> nodes, string path)
    {
        foreach (var node in nodes)
        {
            if (node.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                return node;
            var found = FindNode(node.Children, path);
            if (found != null) return found;
        }
        return null;
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
