using H.Account.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace H.Account.Application;

/// <summary>
/// 企业级用户管理服务实现
/// </summary>
public class AccountUserAppService : ApplicationService, IAccountUserAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;
    private readonly ICurrentTenant _currentTenant;

    public AccountUserAppService(
        IdentityUserManager userManager,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository,
        ICurrentTenant currentTenant)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _currentTenant = currentTenant;
    }

    public async Task<UserDto?> GetUserDtoByIdAsync(Guid userId)
    {
        using var _ = _currentTenant.Change(null);
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<PagedResult<UserDto>> GetPagedUsersAsync(UserQueryParams queryParams)
    {
        using var _ = _currentTenant.Change(null);
        var query = await _userRepository.GetListAsync();

        // 关键字搜索
        if (!string.IsNullOrWhiteSpace(queryParams.Keyword))
        {
            query = query.Where(u => u.UserName!.Contains(queryParams.Keyword) ||
                                     u.Email!.Contains(queryParams.Keyword) ||
                                     (u.PhoneNumber != null && u.PhoneNumber.Contains(queryParams.Keyword))).ToList();
        }

        // 状态筛选
        if (queryParams.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == queryParams.IsActive.Value).ToList();
        }

        var total = query.Count();
        var items = query
            .Skip((queryParams.PageIndex - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(MapToUserDto)
            .ToList();

        return new PagedResult<UserDto>
        {
            Items = items,
            Total = total,
            PageIndex = queryParams.PageIndex,
            PageSize = queryParams.PageSize
        };
    }

    public async Task ResetPasswordAsync(Guid userId, ResetPasswordDto dto)
    {
        using var _ = _currentTenant.Change(null);

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

    private static UserDto MapToUserDto(Volo.Abp.Identity.IdentityUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
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
}
