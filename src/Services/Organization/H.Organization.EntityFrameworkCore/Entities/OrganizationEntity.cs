namespace H.Organization.EntityFrameworkCore;

/// <summary>
/// 部门/组织
/// </summary>
public class OrganizationEntity
{
    /// <summary>
    /// 部门ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 父部门ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 部门名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 部门编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 部门负责人ID（关联Account服务的用户）
    /// </summary>
    public Guid? LeaderId { get; set; }

    /// <summary>
    /// 联系电话
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

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
    /// 父部门
    /// </summary>
    public virtual OrganizationEntity? Parent { get; set; }

    /// <summary>
    /// 子部门
    /// </summary>
    public virtual ICollection<OrganizationEntity> Children { get; set; } = new List<OrganizationEntity>();

    /// <summary>
    /// 部门成员
    /// </summary>
    public virtual ICollection<MemberEntity> Members { get; set; } = new List<MemberEntity>();
}
