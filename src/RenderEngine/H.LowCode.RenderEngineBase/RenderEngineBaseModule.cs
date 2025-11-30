using H.LowCode.Application.Contracts;
using H.LowCode.RenderEngineBase;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngineBase;

public class RenderEngineBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册应用上下文服务
        context.Services.AddScoped<ICurrentApp, RenderEngineCurrentApp>();
    }
}
