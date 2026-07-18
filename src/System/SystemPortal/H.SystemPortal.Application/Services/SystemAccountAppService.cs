using H.SystemPortal.Application.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace H.SystemPortal.Application;

public class SystemAccountAppService : ApplicationService, ISystemAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SystemAccountAppService(
        IdentityUserManager userManager,
        ICurrentTenant currentTenant,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _currentTenant = currentTenant;
        _httpContextAccessor = httpContextAccessor;
    }

    private const string SystemCookieScheme = "SystemCookies";

    [IgnoreAntiforgeryToken]
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
            return new AuthResponseDto { Success = false, Message = "用户名或密码错误" };

        if (!user.IsActive)
            return new AuthResponseDto { Success = false, Message = "账户已被禁用" };

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return new AuthResponseDto { Success = false, Message = "用户名或密码错误" };

        // 验证是否拥有系统管理员角色
        IList<string>? roles = null;
        using (_currentTenant.Change(null))
        {
            roles = await _userManager.GetRolesAsync(user);
        }
        if (roles == null || roles.Count == 0)
            return new AuthResponseDto { Success = false, Message = "无权限访问" };

        var isSystemAdmin = roles.Any(r =>
            r.Equals(SystemRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
            return new AuthResponseDto { Success = false, Message = "无权限访问" };

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

        // 设置系统Cookie认证
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, SystemCookieScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(24)
            };

            await httpContext.SignInAsync(
                SystemCookieScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            User = userDto
        };
    }

    [IgnoreAntiforgeryToken]
    public async Task SystemLogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(SystemCookieScheme);
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // 显式认证 SystemCookies 方案（UseAuthentication 只认证默认方案）
        var authResult = await httpContext.AuthenticateAsync(SystemCookieScheme);
        if (!authResult.Succeeded || authResult.Principal == null)
            return null;

        // 从认证结果中获取用户 ID
        var userIdClaim = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return null;

        var userId = Guid.Parse(userIdClaim.Value);

        // 系统用户跨租户查找
        using (_currentTenant.Change(null))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || !user.IsActive)
                return null;

            // 验证是否拥有系统管理员角色
            var roles = await _userManager.GetRolesAsync(user);
            var isSystemAdmin = roles.Any(r =>
                r.Equals(SystemRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                r.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase));

            if (!isSystemAdmin)
                return null;

            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                RoleNames = roles.ToList(),
                LoginMode = "System",
                IsActive = true
            };
        }
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
