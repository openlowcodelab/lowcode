using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.File.EntityFrameworkCore;

/// <summary>
/// 文件对象实体（存储 MinIO 中文件的元数据，用于避免查询时调用 MinIO）
/// </summary>
public class FileObjectEntity : CreationAuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目 ID</summary>
    public Guid ProjectId { get; set; }

    /// <summary>文件在 MinIO 中的完整路径（Key）</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>文件名（不含路径）</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>文件大小（字节）</summary>
    public long Size { get; set; }

    /// <summary>MIME 类型</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>文件所在的分类路径（以 / 结尾，根目录为空字符串）</summary>
    public string FolderPath { get; set; } = string.Empty;
}
