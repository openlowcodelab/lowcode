using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application;

public class RenderEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IRenderEngineAppService, RenderEngineAppService>();

        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
