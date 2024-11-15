using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.HttpApi;

[DependsOn(typeof(RenderEngineApplicationModule))]
public class RenderEngineHttpApiModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddControllers().AddApplicationPart(typeof(RenderEngineHttpApiModule).Assembly);

        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<List<SiteOption>>(configuration.GetSection(SiteOption.SectionName));
    }
}
