using H.Account.Application;
using H.Account.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace H.Account.HttpApi;

/// <summary>
/// HttpApi 层服务注册扩展
/// </summary>
public static class HttpApiServiceCollectionExtensions
{
    /// <summary>
    /// 添加 HttpApi 层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <param name="jwtKey">JWT 密钥</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddAccountHttpApi(
        this IServiceCollection services,
        IConfiguration configuration,
        string? jwtKey = null)
    {
        //注册 Application 服务
        services.AddAccountApplication(configuration);

        services.AddControllers()
            .AddApplicationPart(typeof(HttpApiServiceCollectionExtensions).Assembly);


        var key = jwtKey ?? configuration["Jwt:SecretKey"] ?? "ThisIsASecretKeyThatIsAtLeast32BytesLongForHS256Signing";

        // 配置 JWT 认证
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(key)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        // 配置授权策略
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireClaim("UserType", "1", "2"));
            options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("UserType", "2"));
        });

        return services;
    }
}
