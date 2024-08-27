using H.LowCode.RenderEngine.Application.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application
{
    public class RenderEngineApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IRenderEngineAppService, RenderEngineAppService>();
        }
    }
}
