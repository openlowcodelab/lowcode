using H.LowCode.RenderEngine.Application;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.HttpApi
{
    [DependsOn(typeof(RenderEngineApplicationModule))]
    internal class RenderEngineHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddControllers().AddApplicationPart(typeof(RenderEngineHttpApiModule).Assembly);
        }
    }
}
