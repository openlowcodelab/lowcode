using H.LowCode.DesignEngine.Application.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Application
{
    public class DesignEngineApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignEngineAppService, DesignEngineAppService>();

            var configuration = context.Services.GetConfiguration();
            context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
        }
    }
}
