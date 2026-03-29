using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using H.Account.Application.Contracts;

namespace H.Account.Application;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthService(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
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

        // 检查用户名是否已存在
        var existingUser = await _userService.GetUserByUserNameAsync(request.UserName);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "用户名已存在"
            };
        }

        // 检查邮箱是否已存在
        existingUser = await _userService.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "邮箱已存在"
            };
        }

        // 创建新用户
        var user = new UserDto
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Password = request.Password,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userService.CreateUserAsync(user);
        if (createdUser == null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "注册失败，请稍后再试"
            };
        }

        // 生成JWT令牌
        var token = GenerateJwtToken(createdUser);

        return new AuthResponseDto
        {
            Success = true,
            Message = "注册成功",
            Token = token,
            User = createdUser
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userService.GetUserByUserNameAsync(request.UserName);
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

        if (!await _userService.VerifyPasswordAsync(request.UserName, request.Password))
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        // 更新最后登录时间
        await _userService.UpdateLastLoginTimeAsync(user.Id);

        // 生成JWT令牌
        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "登录成功",
            Token = token,
            User = user
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        return user;
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string GenerateJwtToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? "ThisIsASecretKeyThatIsAtLeast32BytesLongForHS256Signing");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserType", ((int)user.UserType).ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}