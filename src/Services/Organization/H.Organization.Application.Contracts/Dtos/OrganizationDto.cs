namespace H.Organization.Application.Contracts;

/// <summary>
/// 部门/组织 DTO
/// </summary>
public class OrganizationDto
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
    /// 部门负责人ID
    /// </summary>
    public Guid? LeaderId { get; set; }

    /// <summary>
    /// 部门负责人名称
    /// </summary>
    public string? LeaderName { get; set; }

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
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 子部门数量
    /// </summary>
    public int ChildrenCount { get; set; }

    /// <summary>
    /// 成员数量
    /// </summary>
    public int MembersCount { get; set; }
}

/// <summary>
/// 创建部门请求
/// </summary>
public class CreateOrganizationDto
{
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
    /// 部门负责人ID
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
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新部门请求
/// </summary>
public class UpdateOrganizationDto
{
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
    /// 部门负责人ID
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
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 部门查询参数
/// </summary>
public class OrganizationQueryParams
{
    /// <summary>
    /// 父部门ID（查询子部门）
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 关键字（名称或编码）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// 树形部门节点
/// </summary>
public class OrganizationTreeDto
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
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 子部门
    /// </summary>
    public List<OrganizationTreeDto> Children { get; set; } = new();
}
