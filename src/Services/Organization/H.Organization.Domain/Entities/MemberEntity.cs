namespace H.Organization.Domain;

/// <summary>
/// 成员（关联Account服务的用户）
/// </summary>
public class MemberEntity
{
    /// <summary>
    /// 成员ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 部门ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 用户ID（来自Account服务）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 成员名称（冗余存储Account服务的用户名）
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; } = 1;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否为主部门（一个用户可能有多个部门，主部门唯一）
    /// </summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 更新者ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 部门
    /// </summary>
    public virtual OrganizationEntity? Organization { get; set; }
}
