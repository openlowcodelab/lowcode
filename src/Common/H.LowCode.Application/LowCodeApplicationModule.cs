using H.LowCode.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Application;

public class LowCodeApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HTTP 上下文访问器
        context.Services.AddHttpContextAccessor();

        // 注册应用上下文服务
        context.Services.AddScoped<IAppContextService, AppContextService>();

        // 注册 AppId 请求头拦截器
        context.Services.AddTransient<AppIdHeaderInterceptor>();

        // 为 HttpClient 添加 AppId 请求头拦截器
        context.Services.AddHttpClient("default")
            .AddHttpMessageHandler<AppIdHeaderInterceptor>();
    }
}
