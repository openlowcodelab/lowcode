using H.LowCode.Components.Defaults;
using H.LowCode.DesignEngine;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngineBase;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Workbench;

[DependsOn(
    typeof(DesignEngineBaseModule),
    typeof(DesignEngineModule),
    typeof(MyAppModule),
    typeof(PartsDesignEngineModule),
    typeof(LowCodeDefaultComponentModule)
)]
public class LowCodeWorkbenchModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();
    }
}
