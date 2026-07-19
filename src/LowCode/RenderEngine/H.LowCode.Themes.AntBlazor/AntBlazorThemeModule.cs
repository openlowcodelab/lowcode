using H.LowCode.RenderEngine;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngineBase;
using Volo.Abp.Modularity;

namespace H.LowCode.Themes.AntBlazor;

[DependsOn(
    typeof(RenderEngineBaseModule),
    typeof(RenderEngineModule),
    //Contracts
    typeof(RenderEngineApplicationContractsModule)
)]
public class AntBlazorThemeModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
    }
}
