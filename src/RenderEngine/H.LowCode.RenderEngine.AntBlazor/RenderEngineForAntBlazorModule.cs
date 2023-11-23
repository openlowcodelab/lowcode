using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.AntBlazor
{
    public class RenderEngineForAntBlazorModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAntDesign();
            context.Services.AddScoped<IRender, AntBlazorRender>();
        }
    }
}
