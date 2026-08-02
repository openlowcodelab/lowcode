using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.File.EntityFrameworkCore;

/// <summary>
/// 文件项目实体（对应 MinIO Bucket 的元数据）
/// </summary>
public class FileProjectEntity : AuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>项目名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>项目描述</summary>
    public string? Description { get; set; }

    /// <summary>图标</summary>
    public string? Icon { get; set; }

    /// <summary>MinIO Bucket 名称（全局唯一，含租户前缀）</summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>文件数量（缓存值，上传/删除时同步更新）</summary>
    public int FileCount { get; set; }

    /// <summary>文件总大小（缓存值，上传/删除时同步更新）</summary>
    public long TotalSize { get; set; }
}
