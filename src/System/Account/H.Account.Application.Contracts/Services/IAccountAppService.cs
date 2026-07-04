using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

public interface IAccountAppService : IApplicationService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> SystemLoginAsync(LoginRequestDto request);
    Task<UserDto?> GetCurrentUserAsync();
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync();
}
