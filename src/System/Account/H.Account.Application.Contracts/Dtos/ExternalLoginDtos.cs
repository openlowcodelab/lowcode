using H.SystemPortal.Application.Contracts;

namespace H.Account.Application.Contracts;

/// <summary>
/// 外部登录请求 DTO
/// </summary>
public class ExternalLoginRequestDto
{
    /// <summary>
    /// 登录提供者（WeChat / DingTalk）
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// 提供者的用户唯一标识（openid）
    /// </summary>
    public string ProviderKey { get; set; } = string.Empty;

    /// <summary>
    /// 显示名称（昵称）
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 头像 URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 统一用户标识（unionid）
    /// </summary>
    public string? UnionId { get; set; }
}

/// <summary>
/// 外部登录结果 DTO
/// </summary>
public class ExternalLoginResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 是否为新注册用户
    /// </summary>
    public bool IsNewUser { get; set; }

    public UserDto? User { get; set; }
}
