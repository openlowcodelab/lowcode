using H.LowCode.DesignEngine.Application.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Application
{
    public class DesignEngineApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IDesignEngineAppService, DesignEngineAppService>();
        }
    }
}
