using H.Account.Application;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp.Modularity;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;

namespace H.Account.HttpApi;

[DependsOn(
    typeof(AccountApplicationModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpAutofacModule)
)]
public class AccountHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        // 配置控制器
        context.Services.AddControllers()
            .AddApplicationPart(typeof(AccountHttpApiModule).Assembly);

        // 配置 JWT 认证
        var key = configuration["Jwt:SecretKey"] ?? "ThisIsASecretKeyThatIsAtLeast32BytesLongForHS256Signing";

        context.Services.AddAuthentication(options =>
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
        context.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireClaim("UserType", "1", "2"));
            options.AddPolicy("SuperAdmin", policy => policy.RequireClaim("UserType", "2"));
        });
    }
}
