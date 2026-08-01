using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Notification.EntityFrameworkCore;

/// <summary>
/// 联系人实体
/// </summary>
public class ContactEntity : AuditedEntity<Guid>, IMultiTenant
{
    public ContactEntity()
    {
    }

    public ContactEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 站内信目标用户标识
    /// </summary>
    public string? InAppUserId { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// 所属分组关系
    /// </summary>
    public virtual ICollection<ContactGroupMemberEntity> Memberships { get; set; } = new List<ContactGroupMemberEntity>();
}

/// <summary>
/// 联系人分组实体（主键 long，从 10000 自增）
/// </summary>
public class ContactGroupEntity : AuditedEntity<long>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }

    public virtual ICollection<ContactGroupMemberEntity> Members { get; set; } = new List<ContactGroupMemberEntity>();
}

/// <summary>
/// 联系人-分组成员关系实体
/// </summary>
public class ContactGroupMemberEntity : Entity<Guid>, IMultiTenant
{
    public ContactGroupMemberEntity()
    {
    }

    public ContactGroupMemberEntity(Guid id) : base(id)
    {
    }

    public virtual Guid? TenantId { get; set; }

    public Guid ContactId { get; set; }

    public long GroupId { get; set; }

    public virtual ContactEntity? Contact { get; set; }

    public virtual ContactGroupEntity? Group { get; set; }
}
