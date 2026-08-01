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
    public string Name { get; set; } = string.Empty;
    public List<FileFolderDto> Children { get; set; } = new();
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
}
