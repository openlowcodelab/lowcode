using H.LowCode.Configuration;
using H.LowCode.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application;

[DependsOn(typeof(LowCodeDomainModule))]
public class RenderEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
