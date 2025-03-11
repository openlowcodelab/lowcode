using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Application;

[DependsOn(typeof(DesignEngineDomainModule))]
public class DesignEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<List<SiteOption>>(configuration.GetSection(SiteOption.SectionName));
    }
}
