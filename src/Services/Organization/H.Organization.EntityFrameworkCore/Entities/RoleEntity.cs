namespace H.Organization.EntityFrameworkCore;

/// <summary>
/// 角色
/// </summary>
public class RoleEntity
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 角色类型：1-系统角色 2-自定义角色
    /// </summary>
    public int RoleType { get; set; } = 2;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否为数据范围（全部数据/本部门数据/仅本人数据）
    /// </summary>
    public int DataScope { get; set; } = 1;

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
    /// 角色成员
    /// </summary>
    public virtual ICollection<RoleMember> RoleMembers { get; set; } = new List<RoleMember>();
}

/// <summary>
/// 角色成员关联
/// </summary>
public class RoleMember
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 成员ID
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 角色
    /// </summary>
    public virtual RoleEntity? Role { get; set; }

    /// <summary>
    /// 成员
    /// </summary>
    public virtual MemberEntity? Member { get; set; }
}
