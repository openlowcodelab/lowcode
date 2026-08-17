using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Account.Application.Contracts;

public interface IAccountAppService : IAppService
{
    Task<BaseOutput<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<BaseOutput<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<BaseOutput<UserDto?>> GetUserByIdAsync(Guid userId);
    Task<BaseOutput<bool>> ValidateTokenAsync(string token);
    Task<BaseOutput> LogoutAsync();

    /// <summary>
    /// 获取当前登录的企业用户信息
    /// </summary>
    Task<BaseOutput<UserDto?>> GetCurrentUserAsync();
}
