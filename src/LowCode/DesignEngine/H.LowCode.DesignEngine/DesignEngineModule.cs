using H.LowCode.DesignEngineBase;
using H.LowCode.MetaSchema.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine;

public class DesignEngineModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAntDesign();

        context.Services.AddSingleton(typeof(DragDropStateService));
        context.Services.AddTransient<IFormValidationService, FormValidationService>();
    }
}
