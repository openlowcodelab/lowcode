using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.File.EntityFrameworkCore;

/// <summary>
/// 分类实体（对应 MinIO 中一个目录的元数据）
/// </summary>
public class FileFolderEntity : AuditedEntity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属项目</summary>
    public virtual Guid ProjectId { get; set; }

    /// <summary>分类编号（3-20 位小写字母，作为 MinIO 目录名）</summary>
    public virtual string Code { get; set; } = string.Empty;

    /// <summary>分类显示名称</summary>
    public virtual string Name { get; set; } = string.Empty;

    /// <summary>分类在 MinIO 中的完整目录前缀（以 / 结尾），父级为空字符串表示根分类</summary>
    public virtual string Path { get; set; } = string.Empty;
}
