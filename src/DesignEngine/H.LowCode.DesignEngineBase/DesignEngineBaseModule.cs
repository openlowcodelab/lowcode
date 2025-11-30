using H.LowCode.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngineBase;

public class DesignEngineBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册应用上下文服务
        context.Services.AddScoped<ICurrentApp, DesignEngineCurrentApp>();
    }
}
