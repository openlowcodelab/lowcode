using H.SystemPortal.Application.Contracts;
using H.Util.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Volo.Abp.Application.Services;

namespace H.SystemPortal.Application;

public class SystemAccountAppService : ApplicationService, ISystemAccountAppService
{
    private readonly SystemUserStore _store;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SystemAccountAppService(
        SystemUserStore store,
        IHttpContextAccessor httpContextAccessor)
    {
        _store = store;
        _httpContextAccessor = httpContextAccessor;
    }

    private const string SystemCookieScheme = "SystemCookies";

    public async Task<BaseOutput<AuthResponseDto>> SystemLoginAsync(LoginRequestDto request)
    {
        var loginType = DetectLoginType(request.Account);

        SystemUserEntity? user = loginType switch
        {
            LoginType.Email => _store.FindByEmail(request.Account),
            LoginType.PhoneNumber => _store.FindByPhoneNumber(request.Account),
            _ => _store.FindByUserName(request.Account)
        };

        if (user == null)
            return BaseOutput<AuthResponseDto>.Ok(new AuthResponseDto { Success = false, Message = "用户名或密码错误" });

        if (!user.IsActive)
            return BaseOutput<AuthResponseDto>.Ok(new AuthResponseDto { Success = false, Message = "账户已被禁用" });

        if (!_store.VerifyPassword(user, request.Password))
            return BaseOutput<AuthResponseDto>.Ok(new AuthResponseDto { Success = false, Message = "用户名或密码错误" });

        // 验证是否拥有系统管理员角色
        var isSystemAdmin = user.RoleNames.Any(r =>
            r.Equals(SystemRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
            return BaseOutput<AuthResponseDto>.Ok(new AuthResponseDto { Success = false, Message = "无权限访问" });

        // 更新最后登录时间
        user.LastLoginAt = DateTime.UtcNow;
        _store.Update(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleNames = user.RoleNames,
            UserType = user.UserType,
            LoginMode = "System",
            IsActive = true
        };

        // 设置系统Cookie认证
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email)
            };

            foreach (var role in user.RoleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, SystemCookieScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(24)
            };

            await httpContext.SignInAsync(
                SystemCookieScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return BaseOutput<AuthResponseDto>.Ok(new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            User = userDto
        });
    }

    public async Task<BaseOutput> SystemLogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(SystemCookieScheme);
        }
        return BaseOutput.Ok();
    }

    public Task<BaseOutput<UserDto?>> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return Task.FromResult(BaseOutput<UserDto?>.Ok((UserDto?)null));

        // 显式认证 SystemCookies 方案
        var authResult = httpContext.AuthenticateAsync(SystemCookieScheme).GetAwaiter().GetResult();
        if (!authResult.Succeeded || authResult.Principal == null)
            return Task.FromResult(BaseOutput<UserDto?>.Ok((UserDto?)null));

        var userIdClaim = authResult.Principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Task.FromResult(BaseOutput<UserDto?>.Ok((UserDto?)null));

        var user = _store.FindById(userId);
        if (user == null || !user.IsActive)
            return Task.FromResult(BaseOutput<UserDto?>.Ok((UserDto?)null));

        var isSystemAdmin = user.RoleNames.Any(r =>
            r.Equals(SystemRoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(SystemRoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isSystemAdmin)
            return Task.FromResult(BaseOutput<UserDto?>.Ok((UserDto?)null));

        var dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            RoleNames = user.RoleNames,
            UserType = user.UserType,
            LoginMode = "System",
            IsActive = true
        };

        return Task.FromResult(BaseOutput<UserDto?>.Ok(dto));
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
