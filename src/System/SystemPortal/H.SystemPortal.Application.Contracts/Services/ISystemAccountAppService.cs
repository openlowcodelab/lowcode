using Volo.Abp.Application.Services;

namespace H.SystemPortal.Application.Contracts;

public interface ISystemAccountAppService : IApplicationService
{
    /// <summary>
    /// 验证系统用户登录凭据，成功返回 UserDto，失败返回 null
    /// </summary>
    Task<AuthResponseDto> SystemLoginAsync(LoginRequestDto request);
}
