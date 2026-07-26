using H.Abp.Application.Contracts;

namespace H.SystemPortal.Application.Contracts;

public interface ISystemAccountAppService : IAppService
{
    /// <summary>
    /// 验证系统用户登录凭据，成功返回 UserDto，失败返回 null
    /// </summary>
    Task<AuthResponseDto> SystemLoginAsync(LoginRequestDto request);

    /// <summary>
    /// 获取当前登录的系统用户信息
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>
    /// 系统用户登出
    /// </summary>
    Task SystemLogoutAsync();
}
