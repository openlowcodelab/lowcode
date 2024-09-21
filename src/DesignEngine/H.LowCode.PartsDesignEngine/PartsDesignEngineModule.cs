using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.PartsDesignEngine;

public class PartsDesignEngineModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();
        context.Services.AddSingleton(typeof(DragDropStateService));
    }
}
