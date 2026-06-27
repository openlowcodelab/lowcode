using H.LowCode.Components.Defaults;
using H.LowCode.DesignEngineBase;
using Volo.Abp.Modularity;

namespace H.LowCode.Workbench;

[DependsOn(
    typeof(DesignEngineBaseModule),
    typeof(LowCodeDefaultComponentModule)
)]
public class LowCodeWorkbenchModule : AbpModule
{
}
