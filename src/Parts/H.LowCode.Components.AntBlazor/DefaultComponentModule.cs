using H.LowCode.DesignEngine;
using H.LowCode.DesignEngine.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Components.AntBlazor;

public class DefaultComponentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();
    }
}
