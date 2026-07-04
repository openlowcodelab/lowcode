using Volo.Abp.Application.Services;

namespace H.Account.Application.Contracts;

public interface IUserAppService : IApplicationService
{
    Task<UserDto?> GetUserByUserNameAsync(string userName);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto> CreateUserAsync(UserDto user);
    Task<bool> UpdateUserAsync(UserDto user);
    Task<UserDto?> GetUserByIdAsync(Guid userId);

    // 用户管理方法
    Task<PagedResult<UserDto>> GetPagedUsersAsync(UserQueryParams queryParams);
    Task<UserDto?> GetUserDtoByIdAsync(Guid userId);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, Guid? currentUserId = null);
    Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? currentUserId = null);
    Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? currentUserId = null);
    Task ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
    Task DeleteUserAsync(Guid userId);
    Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId = null);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task<bool> VerifyPasswordAsync(string userName, string password);
    Task UpdateLastLoginTimeAsync(Guid userId);

    // 角色管理
    Task AssignRolesToUserAsync(Guid userId, List<string> roleNames);
    Task<List<string>> GetUserRoleNamesAsync(Guid userId);
}
