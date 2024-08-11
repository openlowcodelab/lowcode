using H.LowCode.RenderEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.ComponentParts.BasicComponents.Render
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
