using H.LowCode.DesignEngine.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.HttpApi
{
    [DependsOn(typeof(DesignEngineApplicationModule))]
    internal class DesignEngineHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(DesignEngineHttpApiModule).Assembly);
        }
    }
}
