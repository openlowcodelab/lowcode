using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.SystemPortal.Application.Contracts;

public interface IUserAppService : IAppService
{
    Task<BaseOutput<UserDto?>> GetUserByUserNameAsync(string userName);
    Task<BaseOutput<UserDto?>> GetUserByEmailAsync(string email);
    Task<BaseOutput<UserDto>> CreateUserAsync(UserDto user);
    Task<BaseOutput<bool>> UpdateUserAsync(UserDto user);
    Task<BaseOutput<UserDto?>> GetUserByIdAsync(Guid userId);

    // 用户管理方法
    Task<BaseOutput<PagedResult<UserDto>>> GetPagedUsersAsync(UserQueryParams queryParams);
    Task<BaseOutput<UserDto?>> GetUserDtoByIdAsync(Guid userId);
    Task<BaseOutput<UserDto>> CreateUserAsync(CreateUserDto dto, Guid? currentUserId = null);
    Task<BaseOutput<bool>> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? currentUserId = null);
    Task<BaseOutput> UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? currentUserId = null);
    Task<BaseOutput> ResetPasswordAsync(Guid userId, ResetPasswordDto dto);
    Task<BaseOutput> DeleteUserAsync(Guid userId);
    Task<BaseOutput<bool>> ExistsByUserNameAsync(string userName, Guid? excludeId = null);
    Task<BaseOutput<bool>> ExistsByEmailAsync(string email, Guid? excludeId = null);
    Task<BaseOutput<bool>> VerifyPasswordAsync(string userName, string password);
    Task<BaseOutput> UpdateLastLoginTimeAsync(Guid userId);

    // 角色管理
    Task<BaseOutput> AssignRolesToUserAsync(Guid userId, List<string> roleNames);
    Task<BaseOutput<List<string>>> GetUserRoleNamesAsync(Guid userId);
}
