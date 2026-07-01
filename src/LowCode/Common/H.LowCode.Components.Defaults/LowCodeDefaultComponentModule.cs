using H.LowCode.ComponentBase;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Components.Defaults;

[DependsOn(typeof(ComponentBaseModule))]
public class LowCodeDefaultComponentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
    }
}
