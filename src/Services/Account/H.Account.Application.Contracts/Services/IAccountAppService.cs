using H.SystemPortal.Application.Contracts;
using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

public interface IAccountAppService : IApplicationService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync();
}
