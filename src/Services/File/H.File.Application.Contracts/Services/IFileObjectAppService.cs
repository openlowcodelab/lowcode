using H.Abp.Application.Contracts;

namespace H.File.Application.Contracts;

/// <summary>
/// 文件对象应用服务接口（文件管理，操作 MinIO 中的文件）
/// </summary>
public interface IFileObjectAppService : IAppService
{
    /// <summary>获取指定项目下某路径的文件列表</summary>
    Task<List<FileObjectDto>> GetFilesAsync(Guid projectId, string? folderPath = null);

    /// <summary>获取指定项目下的文件夹树</summary>
    Task<List<FileFolderDto>> GetFolderTreeAsync(Guid projectId);

    /// <summary>上传文件</summary>
    Task<FileUploadResultDto> UploadAsync(Guid projectId, string folderPath, string fileName, byte[] content, string? contentType = null);

    /// <summary>获取文件下载URL</summary>
    Task<string> GetDownloadUrlAsync(Guid projectId, string fileKey);

    /// <summary>获取文件预览信息</summary>
    Task<FilePreviewDto> GetPreviewAsync(Guid projectId, string fileKey);

    /// <summary>删除文件</summary>
    Task DeleteAsync(Guid projectId, string fileKey);

    /// <summary>创建分类（编号为 MinIO 目录名，留空自动生成）</summary>
    Task CreateFolderAsync(Guid projectId, CreateFolderInput input);

    /// <summary>重命名分类（仅修改显示名称，分类编号不变）</summary>
    Task RenameFolderAsync(Guid projectId, string folderPath, string newName);

    /// <summary>删除文件夹（含其中所有文件）</summary>
    Task DeleteFolderAsync(Guid projectId, string folderPath);
}
