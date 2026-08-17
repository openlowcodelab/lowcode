using H.Account.Application.Contracts;
using H.Util.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Volo.Abp.Application.Services;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace H.Account.Application;

public class AccountAppService : ApplicationService, IAccountAppService
{
    private readonly IdentityUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    public AccountAppService(
        IdentityUserManager userManager,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    [IgnoreAntiforgeryToken]
    public async Task<BaseOutput<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        // 验证密码确认
        if (request.Password != request.ConfirmPassword)
        {
            return new(new AuthResponseDto
            {
                Success = false,
                Message = "密码和确认密码不匹配"
            });
        }

        IdentityUser user;

        switch (request.RegisterType)
        {
            case RegisterType.Email:
                if (string.IsNullOrWhiteSpace(request.Email))
                    return new(new AuthResponseDto { Success = false, Message = "邮箱不能为空" });

                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                    return new(new AuthResponseDto { Success = false, Message = "邮箱已被注册" });

                user = new IdentityUser(_guidGenerator.Create(), request.Email, request.Email);
                break;

            case RegisterType.PhoneNumber:
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    return new(new AuthResponseDto { Success = false, Message = "手机号不能为空" });

                var allUsers = await _userManager.Users.ToListAsync();
                var existingPhone = allUsers.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
                if (existingPhone != null)
                    return new(new AuthResponseDto { Success = false, Message = "手机号已被注册" });

                user = new IdentityUser(_guidGenerator.Create(), request.PhoneNumber, $"{request.PhoneNumber}@temp.local");
                user.SetPhoneNumber(request.PhoneNumber, false);
                break;

            default: // UserName
                if (string.IsNullOrWhiteSpace(request.UserName))
                    return new(new AuthResponseDto { Success = false, Message = "用户名不能为空" });

                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null)
                    return new(new AuthResponseDto { Success = false, Message = "用户名已存在" });

                user = new IdentityUser(_guidGenerator.Create(), request.UserName, request.Email ?? $"{request.UserName}@temp.local");
                break;
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new(new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }

        return new(new AuthResponseDto
        {
            Success = true,
            Message = "注册成功",
            User = await MapToUserDtoAsync(user)
        });
    }

    [IgnoreAntiforgeryToken]
    public async Task<BaseOutput<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        IdentityUser? user = null;

        // 自动判断输入格式
        var loginType = DetectLoginType(request.Account);

        // 登录时使用 Host 上下文，跨租户查找用户
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
        {
            return new(new AuthResponseDto
            {
                Success = false,
                Message = "用户名或密码错误"
            });
        }

        if (!user.IsActive)
        {
            return new(new AuthResponseDto
            {
                Success = false,
                Message = "账户已被禁用"
            });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return new(new AuthResponseDto
            {
                Success = false,
                Message = "用户名或密码错误"
            });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // 更新最后登录时间
        user.LastModificationTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 设置Cookie认证
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };

            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(24)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        return new(new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            User = await MapToUserDtoAsync(user)
        });
    }

    public async Task<BaseOutput<UserDto?>> GetUserByIdAsync(Guid userId)
    {
        // Account 为全局跨租户数据，用户查找需在 Host 上下文进行
        using (_currentTenant.Change(null))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return new(user != null ? await MapToUserDtoAsync(user) : null);
        }
    }

    public async Task<BaseOutput<bool>> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "YourSuperSecretKey12345678901234567890");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return new(true);
        }
        catch
        {
            return new(false);
        }
    }

    [IgnoreAntiforgeryToken]
    public async Task<BaseOutput> LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        return new();
    }

    public async Task<BaseOutput<UserDto?>> GetCurrentUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return new(null);

        var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return new(null);

        // Account 为全局跨租户数据，用户查找需在 Host 上下文进行
        // （企业选择后 Cookie 携带 TenantId，若不切回 Host 上下文会因租户过滤而找不到用户）
        using (_currentTenant.Change(null))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new(null);

            return new(await MapToUserDtoAsync(user));
        }
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "YourSuperSecretKey12345678901234567890");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "1440")),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private Task<UserDto> MapToUserDtoAsync(IdentityUser user)
    {
        var dto = new UserDto
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
        return Task.FromResult(dto);
    }

    /// <summary>
    /// 自动检测登录类型
    /// </summary>
    private LoginType DetectLoginType(string account)
    {
        if (string.IsNullOrWhiteSpace(account))
            return LoginType.Account;

        // 判断是否为邮箱
        if (account.Contains('@') && account.Contains('.'))
        {
            // 简单的邮箱格式验证
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            if (emailRegex.IsMatch(account))
                return LoginType.Email;
        }

        // 判断是否为手机号（11位数字，以1开头）
        if (System.Text.RegularExpressions.Regex.IsMatch(account, @"^1[3-9]\d{9}$"))
            return LoginType.PhoneNumber;

        // 默认为用户名
        return LoginType.Account;
    }
}
