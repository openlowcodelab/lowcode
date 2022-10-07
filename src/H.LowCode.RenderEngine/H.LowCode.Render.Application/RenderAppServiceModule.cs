using H.LowCode.Render.Application.Abstraction.AppServices;
using H.LowCode.Render.Application.AppServices;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Render.Application
{
    //[DependsOn(typeof(RenderDomainServiceModule))]
    public class RenderAppServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddScoped<IRenderAppService, RenderAppService>();
        }
    }
}
