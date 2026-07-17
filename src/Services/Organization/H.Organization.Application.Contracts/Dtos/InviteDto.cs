namespace H.Organization.Application.Contracts;

/// <summary>
/// 创建邀请请求
/// </summary>
public class CreateInviteDto
{
    /// <summary>
    /// 目标部门ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 成员类型：1-普通成员 2-部门负责人 3-部门经理
    /// </summary>
    public int MemberType { get; set; } = 1;

    /// <summary>
    /// 被邀请人手机号（方式1填写；方式2留空）
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 有效期（天）
    /// </summary>
    public int ExpireDays { get; set; } = 7;
}

/// <summary>
/// 邀请结果
/// </summary>
public class InviteDto
{
    /// <summary>
    /// 邀请令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 邀请链接（完整URL）
    /// </summary>
    public string InviteUrl { get; set; } = string.Empty;

    /// <summary>
    /// 目标部门ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 目标部门名称
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// 是否已发送短信
    /// </summary>
    public bool SmsSent { get; set; }
}

/// <summary>
/// 邀请信息（用于确认加入页）
/// </summary>
public class InviteInfoDto
{
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool Valid { get; set; }

    /// <summary>
    /// 失效原因（无效时）
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 目标部门名称
    /// </summary>
    public string OrganizationName { get; set; } = string.Empty;

    /// <summary>
    /// 成员类型
    /// </summary>
    public int MemberType { get; set; }
}
