using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.File.Application.Contracts;

/// <summary>
/// 文件对象应用服务接口（文件管理，操作 MinIO 中的文件）
/// </summary>
public interface IFileObjectAppService : IAppService
{
    /// <summary>获取指定项目下某路径的文件列表</summary>
    Task<BaseOutput<List<FileObjectDto>>> GetFilesAsync(Guid projectId, string? folderPath = null);

    /// <summary>获取指定项目下的文件夹树</summary>
    Task<BaseOutput<List<FileFolderDto>>> GetFolderTreeAsync(Guid projectId);

    /// <summary>上传文件</summary>
    Task<BaseOutput<FileUploadResultDto>> UploadAsync(Guid projectId, string? folderPath = null, string fileName = "", byte[]? content = null, string? contentType = null);

    /// <summary>获取文件下载URL</summary>
    Task<BaseOutput<FileDownloadResultDto>> GetDownloadUrlAsync(Guid projectId, Guid fileId);

    /// <summary>获取文件预览信息</summary>
    Task<BaseOutput<FilePreviewDto>> GetPreviewAsync(Guid projectId, Guid fileId);

    /// <summary>删除文件</summary>
    Task<BaseOutput> DeleteAsync(Guid projectId, Guid fileId);

    /// <summary>创建分类（编号为 MinIO 目录名，留空自动生成）</summary>
    Task<BaseOutput> CreateFolderAsync(Guid projectId, CreateFolderInput input);

    /// <summary>重命名分类（仅修改显示名称，分类编号不变）</summary>
    Task<BaseOutput> RenameFolderAsync(Guid projectId, string folderPath, string newName);

    /// <summary>删除文件夹（含其中所有文件）</summary>
    Task<BaseOutput> DeleteFolderAsync(Guid projectId, string folderPath);

    /// <summary>初始化分片上传，返回 UploadId 和分片信息</summary>
    Task<BaseOutput<MultipartUploadDto>> InitMultipartUploadAsync(Guid projectId, string? folderPath = null, string fileName = "", long fileSize = 0, string? contentType = null);

    /// <summary>上传单个分片</summary>
    Task<BaseOutput<UploadPartResult>> UploadPartAsync(UploadPartInput input);

    /// <summary>查询已上传的分片（用于断点续传）</summary>
    Task<BaseOutput<List<UploadedPartDto>>> ListUploadedPartsAsync(string uploadId, string objectKey);

    /// <summary>完成分片上传，合并所有分片</summary>
    Task<BaseOutput<FileUploadResultDto>> CompleteMultipartUploadAsync(CompleteMultipartUploadInput input);

    /// <summary>取消分片上传，清理临时数据</summary>
    Task<BaseOutput> AbortMultipartUploadAsync(string uploadId, string objectKey);
}
