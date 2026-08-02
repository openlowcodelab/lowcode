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
            Key = e.Key,
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

    public async Task<FileUploadResultDto> UploadAsync(Guid projectId, string folderPath, string fileName, byte[] content, string? contentType = null)
    {
        var entity = await _repository.GetAsync(projectId);
        var objectKey = string.IsNullOrEmpty(folderPath) ? fileName : $"{EnsureTrailingSlash(folderPath)}{fileName}";
        contentType ??= GetContentType(fileName);

        try
        {
            await _storage.PutObjectAsync(entity.BucketName, objectKey, content, contentType);

            // 写入文件元数据到数据库
            await _objectRepository.InsertAsync(new FileObjectEntity
            {
                ProjectId = projectId,
                Key = objectKey,
                FileName = fileName,
                Size = content.Length,
                ContentType = contentType,
                FolderPath = string.IsNullOrEmpty(folderPath) ? string.Empty : EnsureTrailingSlash(folderPath)
            });

            // 更新项目统计信息
            entity.FileCount += 1;
            entity.TotalSize += content.Length;
            await _repository.UpdateAsync(entity);

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
            // Office 文件：前端使用 docx-preview / xlsx / pptx-preview 等库解析 Base64 内容渲染预览
            preview.PreviewUrl = await _storage.GetPresignedDownloadUrlAsync(entity.BucketName, fileKey, 30);
            var size = await _storage.GetObjectSizeAsync(entity.BucketName, fileKey);
            preview.Size = size ?? 0;
            if (size.HasValue && size.Value <= MaxOfficePreviewSize)
            {
                var data = await _storage.GetObjectAsync(entity.BucketName, fileKey);
                preview.Base64Content = Convert.ToBase64String(data);
            }
        }

        return preview;
    }

    public async Task DeleteAsync(Guid projectId, string fileKey)
    {
        var entity = await _repository.GetAsync(projectId);

        // 获取文件大小
        var fileSize = await _storage.GetObjectSizeAsync(entity.BucketName, fileKey) ?? 0;
        await _storage.RemoveObjectAsync(entity.BucketName, fileKey);

        // 从数据库删除文件元数据
        var queryable = await _objectRepository.GetQueryableAsync();
        var fileEntity = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(o => o.ProjectId == projectId && o.Key == fileKey));
        if (fileEntity != null)
        {
            await _objectRepository.DeleteAsync(fileEntity);
        }

        // 更新项目统计信息
        entity.FileCount -= 1;
        entity.TotalSize -= fileSize;
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

    #region 辅助方法

    /// <summary>Office 文件在线预览的最大大小（超过则提示下载查看）</summary>
    private const long MaxOfficePreviewSize = 30L * 1024 * 1024;

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

    private static string GetFileName(string key)
    {
        var trimmed = key.TrimEnd('/');
        var idx = trimmed.LastIndexOf('/');
        return idx >= 0 ? trimmed[(idx + 1)..] : trimmed;
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
