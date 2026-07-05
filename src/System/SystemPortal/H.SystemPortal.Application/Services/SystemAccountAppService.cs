using H.SystemPortal.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace H.SystemPortal.Application;

public class SystemAccountAppService : ApplicationService, ISystemAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentTenant _currentTenant;

    public SystemAccountAppService(
        IdentityUserManager userManager,
        ICurrentTenant currentTenant)
    {
        _userManager = userManager;
        _currentTenant = currentTenant;
    }

    public async Task<AuthResponseDto> SystemLoginAsync(LoginRequestDto request)
    {
        IdentityUser? user = null;
        var loginType = DetectLoginType(request.Account);

        // 系统用户跨租户查找
        using (_currentTenant.Change(null))
        {
            switch (loginType)
            {
                case LoginType.Email:
                    user = await _userManager.FindByEmailAsync(request.Account);
                    break;
                case LoginType.PhoneNumber:
                    var allUsers = await _userManager.Users.ToListAsync();
                    user = allUsers.FirstOrDefault(u => u.PhoneNumber == request.Account);
                    break;
                default:
                    user = await _userManager.FindByNameAsync(request.Account);
                    break;
            }
        }

        if (user == null)
            return null;

        if (!user.IsActive)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return null;

        // 验证是否拥有系统管理员角色
        var roles = await _userManager.GetRolesAsync(user);
        var isSystemAdmin = roles.Any(r =>
            r.Equals(SystemRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
            return null;

        // 更新最后登录时间
        user.LastModificationTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            RoleNames = roles.ToList(),
            LoginMode = "System",
            IsActive = true
        };

        return new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            User = userDto
        };
    }

    private static LoginType DetectLoginType(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
            return LoginType.Account;

        if (account.Contains('@') && account.Contains('.'))
        {
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (emailRegex.IsMatch(account))
                return LoginType.Email;
        }

        if (System.Text.RegularExpressions.Regex.IsMatch(account, @"^1[3-9]\d{9}$"))
            return LoginType.PhoneNumber;

        return LoginType.Account;
    }
}
