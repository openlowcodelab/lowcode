namespace H.File.Application.Contracts;

/// <summary>
/// 文件项目（对应 MinIO Bucket）DTO
/// </summary>
public class FileProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string BucketName { get; set; } = string.Empty;
    public long TotalSize { get; set; }
    public int FileCount { get; set; }
    public DateTime CreationTime { get; set; }
}

public class CreateFileProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
}

public class UpdateFileProjectDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// 文件对象 DTO
/// </summary>
public class FileObjectDto
{
    public string Key { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? ContentType { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsFolder { get; set; }
}

/// <summary>
/// 文件夹树节点
/// </summary>
public class FileFolderDto
{
    public string Path { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<FileFolderDto> Children { get; set; } = new();
}

/// <summary>
/// 创建分类输入
/// </summary>
public class CreateFolderInput
{
    /// <summary>父分类的 MinIO 目录前缀（以 / 结尾，根分类为空）</summary>
    public string? ParentPath { get; set; }

    /// <summary>分类显示名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>分类编号（3-20 位小写字母，留空则自动生成 8 位），作为 MinIO 目录名</summary>
    public string? Code { get; set; }
}

/// <summary>
/// 文件上传结果
/// </summary>
public class FileUploadResultDto
{
    public string Key { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 文件预览信息
/// </summary>
public class FilePreviewDto
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    /// <summary>预览类型: image, text, markdown, html, pdf, office, unsupported</summary>
    public string PreviewType { get; set; } = "unsupported";
    /// <summary>预览URL（用于图片、PDF等可直接预览的文件）</summary>
    public string? PreviewUrl { get; set; }
    /// <summary>文本内容（用于txt、md、html等文本文件）</summary>
    public string? TextContent { get; set; }
    /// <summary>文件内容 Base64（用于 docx/xlsx/pptx 等需要前端解析预览的文件）</summary>
    public string? Base64Content { get; set; }
}

#region 分片上传

/// <summary>
/// 分片大小（5MB）
/// </summary>
public static class MultipartUploadConstants
{
    public const int DefaultPartSize = 5 * 1024 * 1024;
}

/// <summary>
/// 初始化分片上传 - 输出
/// </summary>
public class MultipartUploadDto
{
    /// <summary>MinIO Multipart Upload ID</summary>
    public string UploadId { get; set; } = string.Empty;
    /// <summary>MinIO 对象 Key</summary>
    public string ObjectKey { get; set; } = string.Empty;
    /// <summary>总分片数</summary>
    public int TotalParts { get; set; }
    /// <summary>每片大小（字节）</summary>
    public int PartSize { get; set; }
}

/// <summary>
/// 上传分片 - 输入
/// </summary>
public class UploadPartInput
{
    public string UploadId { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public int PartIndex { get; set; }
    public byte[] Data { get; set; } = [];
}

/// <summary>
/// 上传分片 - 结果
/// </summary>
public class UploadPartResult
{
    public int PartIndex { get; set; }
    public string ETag { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// 完成分片上传 - 输入
/// </summary>
public class CompleteMultipartUploadInput
{
    public Guid ProjectId { get; set; }
    public string UploadId { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ContentType { get; set; }
    public string FolderPath { get; set; } = string.Empty;
    public List<UploadPartResult> Parts { get; set; } = [];
}

/// <summary>
/// 已上传分片信息
/// </summary>
public class UploadedPartDto
{
    public int PartNumber { get; set; }
    public string ETag { get; set; } = string.Empty;
    public long Size { get; set; }
}

#endregion
