using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

public interface IAccountAppService : IApplicationService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync();

    /// <summary>
    /// 获取当前登录的企业用户信息
    /// </summary>
    Task<UserDto?> GetCurrentUserAsync();
}
