using H.Account.Application.Contracts;
using H.SystemPortal.Application.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace H.Account.Application;

/// <summary>
/// 外部登录应用服务实现
/// </summary>
public class ExternalLoginAppService : ApplicationService, IExternalLoginAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGuidGenerator _guidGenerator;

    public ExternalLoginAppService(
        IdentityUserManager userManager,
        IHttpContextAccessor httpContextAccessor,
        IGuidGenerator guidGenerator)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _guidGenerator = guidGenerator;
    }

    public async Task<ExternalLoginResultDto> ExternalLoginAsync(ExternalLoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.ProviderKey))
        {
            return new ExternalLoginResultDto { Success = false, Message = "登录信息不完整" };
        }

        // 1. 查找已绑定的用户
        var user = await _userManager.FindByLoginAsync(request.Provider, request.ProviderKey);
        var isNewUser = false;

        if (user == null)
        {
            // 2. 未绑定 → 自动创建新用户
            var safeNick = SanitizeUserName(request.DisplayName) ?? request.ProviderKey[..Math.Min(8, request.ProviderKey.Length)];
            var userName = $"{request.Provider}_{safeNick}";

            // 确保用户名唯一
            var existingUser = await _userManager.FindByNameAsync(userName);
            if (existingUser != null)
            {
                userName = $"{userName}_{Guid.NewGuid().ToString("N")[..4]}";
            }

            var email = $"{userName}@external.local";
            user = new IdentityUser(_guidGenerator.Create(), userName, email);

            // 生成满足 ABP 密码策略的随机密码
            var randomPassword = $"Ex{Guid.NewGuid():N}!1";
            var createResult = await _userManager.CreateAsync(user, randomPassword);
            if (!createResult.Succeeded)
            {
                return new ExternalLoginResultDto
                {
                    Success = false,
                    Message = $"创建用户失败: {string.Join(", ", createResult.Errors.Select(e => e.Description))}"
                };
            }

            // 绑定外部登录
            var loginInfo = new UserLoginInfo(request.Provider, request.ProviderKey, request.DisplayName);
            var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
            if (!addLoginResult.Succeeded)
            {
                return new ExternalLoginResultDto
                {
                    Success = false,
                    Message = $"绑定外部账号失败: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}"
                };
            }

            isNewUser = true;
        }
        else
        {
            // 3. 已绑定 → 检查用户状态
            if (!user.IsActive)
            {
                return new ExternalLoginResultDto { Success = false, Message = "账户已被禁用" };
            }
        }

        // 4. 更新最后登录时间
        user.LastModificationTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 5. 写入 Cookie 认证
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(ClaimTypes.Email, user.Email ?? ""),
                new("LoginProvider", request.Provider)
            };

            // 加载用户的系统角色并写入 Cookie Claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return new ExternalLoginResultDto
        {
            Success = true,
            Message = isNewUser ? "注册并登录成功" : "登录成功",
            IsNewUser = isNewUser,
            User = await MapToUserDtoAsync(user)
        };
    }

    public async Task<ExternalLoginResultDto> BindExternalAccountAsync(Guid userId, ExternalLoginRequestDto request)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new ExternalLoginResultDto { Success = false, Message = "用户不存在" };
        }

        // 检查该外部账号是否已被其他用户绑定
        var existingUser = await _userManager.FindByLoginAsync(request.Provider, request.ProviderKey);
        if (existingUser != null && existingUser.Id != userId)
        {
            return new ExternalLoginResultDto { Success = false, Message = "该外部账号已被其他用户绑定" };
        }

        // 检查当前用户是否已绑定同类型外部账号
        var existingLogins = await _userManager.GetLoginsAsync(user);
        if (existingLogins.Any(l => l.LoginProvider == request.Provider))
        {
            return new ExternalLoginResultDto { Success = false, Message = $"已绑定{request.Provider}账号" };
        }

        var loginInfo = new UserLoginInfo(request.Provider, request.ProviderKey, request.DisplayName);
        var result = await _userManager.AddLoginAsync(user, loginInfo);
        if (!result.Succeeded)
        {
            return new ExternalLoginResultDto
            {
                Success = false,
                Message = $"绑定失败: {string.Join(", ", result.Errors.Select(e => e.Description))}"
            };
        }

        return new ExternalLoginResultDto { Success = true, Message = "绑定成功" };
    }

    public async Task<bool> UnbindExternalAccountAsync(Guid userId, string provider)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var logins = await _userManager.GetLoginsAsync(user);
        var login = logins.FirstOrDefault(l => l.LoginProvider == provider);
        if (login == null) return false;

        // 确保用户有密码或其他登录方式，防止账号无法登录
        if (!await _userManager.HasPasswordAsync(user) && logins.Count <= 1)
        {
            return false; // 不能解绑唯一的登录方式
        }

        var result = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        return result.Succeeded;
    }

    public async Task<List<ExternalAccountDto>> GetExternalAccountsAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return new List<ExternalAccountDto>();

        var logins = await _userManager.GetLoginsAsync(user);
        return logins.Select(l => new ExternalAccountDto
        {
            Provider = l.LoginProvider,
            ProviderKey = l.ProviderKey,
            DisplayName = l.ProviderDisplayName,
            BoundAt = DateTime.UtcNow // IdentityUserLogin 不存储绑定时间
        }).ToList();
    }

    private async Task<UserDto> MapToUserDtoAsync(IdentityUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber ?? "",
            RoleNames = roles.ToList(),
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

    /// <summary>
    /// 清理用户名中的非法字符
    /// </summary>
    private static string? SanitizeUserName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        // 移除非字母数字和非 ASCII 字符（保留中文等 Unicode 字符）
        var sanitized = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
        return string.IsNullOrWhiteSpace(sanitized) ? null : sanitized[..Math.Min(20, sanitized.Length)];
    }
}
