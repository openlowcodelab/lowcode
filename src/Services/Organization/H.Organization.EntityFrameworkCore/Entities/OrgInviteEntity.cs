using Volo.Abp.MultiTenancy;

namespace H.Organization.EntityFrameworkCore;

/// <summary>
/// 组织邀请（一次性 + 限时令牌）
/// </summary>
public class OrgInviteEntity : IMultiTenant
{
    /// <summary>
    /// 邀请ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 邀请令牌（唯一）
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 目标部门ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; } = 1;

    /// <summary>
    /// 被邀请人手机号（方式1填写；方式2为空）
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已使用（一次性）
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// 使用者用户ID
    /// </summary>
    public Guid? UsedByUserId { get; set; }

    /// <summary>
    /// 使用时间
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 租户 ID（多租户）
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 目标部门
    /// </summary>
    public virtual OrganizationEntity? Organization { get; set; }
}
