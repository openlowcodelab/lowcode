﻿using H.LowCode.DesignEngine.Abstraction;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Components.Extension;

public class ExtensionComponentModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        //context.Services.AddScoped<IComponentProvider, ExtensionComponentProvider>();
        //context.Services.AddScoped<IPageTemplateProvider, ExtensionPageTemplateProvider>();
    }
}
