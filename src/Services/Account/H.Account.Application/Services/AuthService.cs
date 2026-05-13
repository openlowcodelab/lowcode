using H.Account.Application.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;
using Volo.Abp.Guids;

namespace H.Account.Application;

public class AuthService : ApplicationService, IAuthService
{
    private readonly Volo.Abp.Identity.IdentityUserManager _userManager;
    private readonly IConfiguration _configuration;
    private readonly IGuidGenerator _guidGenerator;

    public AuthService(
        Volo.Abp.Identity.IdentityUserManager userManager,
        IConfiguration configuration,
        IGuidGenerator guidGenerator)
    {
        _userManager = userManager;
        _configuration = configuration;
        _guidGenerator = guidGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // 验证密码确认
        if (request.Password != request.ConfirmPassword)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "密码和确认密码不匹配"
            };
        }

        Volo.Abp.Identity.IdentityUser user;

        switch (request.RegisterType)
        {
            case RegisterType.Email:
                if (string.IsNullOrWhiteSpace(request.Email))
                    return new AuthResponseDto { Success = false, Message = "邮箱不能为空" };

                var existingEmail = await _userManager.FindByEmailAsync(request.Email);
                if (existingEmail != null)
                    return new AuthResponseDto { Success = false, Message = "邮箱已被注册" };

                user = new Volo.Abp.Identity.IdentityUser(_guidGenerator.Create(), request.Email, request.Email);
                break;

            case RegisterType.PhoneNumber:
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                    return new AuthResponseDto { Success = false, Message = "手机号不能为空" };

                var allUsers = await _userManager.Users.ToListAsync();
                var existingPhone = allUsers.FirstOrDefault(u => u.PhoneNumber == request.PhoneNumber);
                if (existingPhone != null)
                    return new AuthResponseDto { Success = false, Message = "手机号已被注册" };

                user = new Volo.Abp.Identity.IdentityUser(_guidGenerator.Create(), request.PhoneNumber, $"{request.PhoneNumber}@temp.local");
                user.SetPhoneNumber(request.PhoneNumber, false);
                break;

            default: // UserName
                if (string.IsNullOrWhiteSpace(request.UserName))
                    return new AuthResponseDto { Success = false, Message = "用户名不能为空" };

                var existingUser = await _userManager.FindByNameAsync(request.UserName);
                if (existingUser != null)
                    return new AuthResponseDto { Success = false, Message = "用户名已存在" };

                user = new Volo.Abp.Identity.IdentityUser(_guidGenerator.Create(), request.UserName, request.Email ?? $"{request.UserName}@temp.local");
                break;
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new AuthResponseDto
        {
            Success = true,
            Message = "注册成功",
            User = MapToUserDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        Volo.Abp.Identity.IdentityUser? user = null;

        switch (request.LoginType)
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

        if (user == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "账户已被禁用"
            };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return new AuthResponseDto
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        
        // 更新最后登录时间 (通过设置 LastModificationTime 或使用自定义字段)
        user.LastModificationTime = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // 生成JWT令牌
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            Token = token,
            User = MapToUserDto(user)
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<bool> ValidateTokenAsync(string token)
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
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateJwtToken(Volo.Abp.Identity.IdentityUser user)
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

    private UserDto MapToUserDto(Volo.Abp.Identity.IdentityUser user)
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
