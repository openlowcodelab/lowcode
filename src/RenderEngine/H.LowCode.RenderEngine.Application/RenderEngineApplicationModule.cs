﻿using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application;

[DependsOn(
    //abp
    typeof(AbpAutoMapperModule),
    //lowcode
    typeof(RenderEngineDomainModule)
    )]
public class RenderEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<RenderEngineApplicationModule>();
        });
    }
}
