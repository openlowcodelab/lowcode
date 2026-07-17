namespace H.Organization.Application.Contracts;

/// <summary>
/// 角色 DTO
/// </summary>
public class RoleDto
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
    public int RoleType { get; set; }

    /// <summary>
    /// 角色类型名称
    /// </summary>
    public string RoleTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 数据范围：1-全部数据 2-本部门数据 3-仅本人数据
    /// </summary>
    public int DataScope { get; set; }

    /// <summary>
    /// 数据范围名称
    /// </summary>
    public string DataScopeName { get; set; } = string.Empty;

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
    /// 成员数量
    /// </summary>
    public int MembersCount { get; set; }
}

/// <summary>
/// 角色已分配成员 DTO
/// </summary>
public class RoleMemberDto
{
    /// <summary>
    /// 成员ID
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 部门名称
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;
}

/// <summary>
/// 创建角色请求
/// </summary>
public class CreateRoleDto
{
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
    /// 数据范围：1-全部数据 2-本部门数据 3-仅本人数据
    /// </summary>
    public int DataScope { get; set; } = 1;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新角色请求
/// </summary>
public class UpdateRoleDto
{
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
    public int RoleType { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 数据范围：1-全部数据 2-本部门数据 3-仅本人数据
    /// </summary>
    public int DataScope { get; set; }

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
/// 角色查询参数
/// </summary>
public class RoleQueryParams
{
    /// <summary>
    /// 关键字（名称或编码）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 角色类型
    /// </summary>
    public int? RoleType { get; set; }

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
