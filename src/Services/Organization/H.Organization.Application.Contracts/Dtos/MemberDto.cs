namespace H.Organization.Application.Contracts;

/// <summary>
/// 成员 DTO
/// </summary>
public class MemberDto
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
    /// 部门名称
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 手机号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; }

    /// <summary>
    /// 成员类型名称
    /// </summary>
    public string MemberTypeName { get; set; } = string.Empty;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否为主部门
    /// </summary>
    public bool IsMain { get; set; }

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
}

/// <summary>
/// 添加成员请求
/// </summary>
public class AddMemberDto
{
    /// <summary>
    /// 部门ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 用户ID（来自Account服务）
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; } = 1;

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否为主部门
    /// </summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 更新成员请求
/// </summary>
public class UpdateMemberDto
{
    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 是否为主部门
    /// </summary>
    public bool IsMain { get; set; }

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
/// 成员查询参数
/// </summary>
public class MemberQueryParams
{
    /// <summary>
    /// 部门ID
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// 关键字（用户名）
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 成员类型
    /// </summary>
    public int? MemberType { get; set; }

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
