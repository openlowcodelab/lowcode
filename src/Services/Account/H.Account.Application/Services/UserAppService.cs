using H.Account.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace H.Account.Application;

public class UserAppService : ApplicationService, IUserAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public UserAppService(
        IdentityUserManager userManager,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<UserDto?> GetUserByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user != null ? await MapToUserDtoAsync(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null ? await MapToUserDtoAsync(user) : null;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? await MapToUserDtoAsync(user) : null;
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? await MapToUserDtoAsync(user) : null;
    }

    public async Task<UserDto> CreateUserAsync(UserDto user)
    {
        var identityUser = new Volo.Abp.Identity.IdentityUser(
            GuidGenerator.Create(),
            user.UserName,
            user.Email);

        identityUser.SetPhoneNumber(user.PhoneNumber, false);

        var result = await _userManager.CreateAsync(identityUser, user.Password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 如果需要禁用用户
        if (!user.IsActive)
        {
            await _userManager.SetLockoutEnabledAsync(identityUser, true);
            await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.MaxValue);
        }

        return await MapToUserDtoAsync(identityUser);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, Guid? currentUserId = null)
    {
        var identityUser = new Volo.Abp.Identity.IdentityUser(
            GuidGenerator.Create(),
            dto.UserName,
            dto.Email);

        identityUser.SetPhoneNumber(dto.PhoneNumber, false);

        var result = await _userManager.CreateAsync(identityUser, dto.Password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // 分配角色
        if (dto.RoleNames != null && dto.RoleNames.Any())
        {
            var addRoleResult = await _userManager.AddToRolesAsync(identityUser, dto.RoleNames);
            if (!addRoleResult.Succeeded)
            {
                throw new Exception(string.Join(", ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }

        if (!dto.IsActive)
        {
            await _userManager.SetLockoutEnabledAsync(identityUser, true);
            await _userManager.SetLockoutEndDateAsync(identityUser, DateTimeOffset.MaxValue);
        }

        return await MapToUserDtoAsync(identityUser);
    }

    public async Task<bool> UpdateUserAsync(UserDto user)
    {
        var existingUser = await _userManager.FindByIdAsync(user.Id.ToString());
        if (existingUser == null)
        {
            return false;
        }

        await _userManager.SetUserNameAsync(existingUser, user.UserName);
        await _userManager.SetEmailAsync(existingUser, user.Email);
        await _userManager.SetPhoneNumberAsync(existingUser, user.PhoneNumber);
        
        existingUser.LastModificationTime = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(existingUser);
        return result.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(Guid userId, UpdateUserDto dto, Guid? currentUserId = null)
    {
        var existingUser = await _userManager.FindByIdAsync(userId.ToString());
        if (existingUser == null)
        {
            return false;
        }

        await _userManager.SetUserNameAsync(existingUser, dto.UserName);
        await _userManager.SetEmailAsync(existingUser, dto.Email);
        await _userManager.SetPhoneNumberAsync(existingUser, dto.PhoneNumber);
        
        // 更新角色
        if (dto.RoleNames != null)
        {
            var currentRoles = await _userManager.GetRolesAsync(existingUser);
            var rolesToRemove = currentRoles.Except(dto.RoleNames, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToAdd = dto.RoleNames.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToRemove.Any())
                await _userManager.RemoveFromRolesAsync(existingUser, rolesToRemove);
            if (rolesToAdd.Any())
                await _userManager.AddToRolesAsync(existingUser, rolesToAdd);
        }
        
        existingUser.LastModificationTime = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(existingUser);
        return result.Succeeded;
    }

    public async Task UpdateUserStatusAsync(Guid userId, UpdateUserStatusDto dto, Guid? currentUserId = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        // 使用 Lockout 来启用/禁用用户
        if (dto.IsActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        user.LastModificationTime = DateTime.UtcNow;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
    {
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new Exception("两次密码输入不一致");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, Guid? excludeId = null)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user != null && user.Id != excludeId;
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null && user.Id != excludeId;
    }

    public async Task<bool> VerifyPasswordAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task UpdateLastLoginTimeAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            user.LastModificationTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(UserQueryParams queryParams)
    {
        var query = await _userRepository.GetListAsync();

        // 关键字搜索
        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(u => u.UserName!.Contains(queryParams.Keyword) ||
                                     u.Email!.Contains(queryParams.Keyword) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(queryParams.Keyword))).ToList();
        }

        // 用户类型筛选：根据角色名过滤
        if (queryParams.UserType.HasValue)
        {
            var targetRoleName = queryParams.UserType.Value switch
            {
                UserType.SuperAdmin => SystemRoleNames.SuperAdmin,
                UserType.Admin => SystemRoleNames.Admin,
                _ => null
            };

            if (targetRoleName != null)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(targetRoleName);
                var userIds = usersInRole.Select(u => u.Id).ToHashSet();
                query = query.Where(u => userIds.Contains(u.Id)).ToList();
            }
        }
        
        // 状态筛选
        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == queryParams.IsActive.Value).ToList();
        }

        var total = query.Count();
        var items = query
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize);

        var dtoItems = new List<UserDto>();
        foreach (var item in items)
        {
            dtoItems.Add(await MapToUserDtoAsync(item));
        }

        return new PagedResult<UserDto>
        {
            Items = dtoItems,
            Total = total,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    public async Task AssignRolesToUserAsync(Guid userId, List<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new Exception("用户不存在");

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(roleNames, StringComparer.OrdinalIgnoreCase).ToList();
        var rolesToAdd = roleNames.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();

        if (rolesToRemove.Any())
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (rolesToAdd.Any())
            await _userManager.AddToRolesAsync(user, rolesToAdd);
    }

    public async Task<List<string>> GetUserRoleNamesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    private async Task<UserDto> MapToUserDtoAsync(Volo.Abp.Identity.IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            RoleNames = roles.ToList(),
            UserType = DeriveUserType(roles),
            IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnd = user.LockoutEnd?.DateTime,
            AccessFailedCount = user.AccessFailedCount,
            CreatedAt = user.CreationTime,
            UpdatedAt = user.LastModificationTime,
            LastLoginAt = user.LastModificationTime
        };
    }

    private static UserType DeriveUserType(IList<string> roles)
    {
        if (roles.Contains(SystemRoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            return UserType.SuperAdmin;
        if (roles.Contains(SystemRoleNames.Admin, StringComparer.OrdinalIgnoreCase))
            return UserType.Admin;
        return UserType.Normal;
    }
}
