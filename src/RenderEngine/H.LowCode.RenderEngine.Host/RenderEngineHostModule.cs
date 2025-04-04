﻿using H.LowCode.ComponentBase;
using H.LowCode.Components.AntBlazor;
using H.LowCode.Components.Extension;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.EntityFrameworkCore;
using H.LowCode.RenderEngine.Repository.JsonFile;
using H.LowCode.Themes.AntBlazor;
using H.Util.Blazor;
using Microsoft.OpenApi.Models;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace H.LowCode.RenderEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpSwashbuckleModule),
    //=====lowcode-server=====//
    typeof(RenderEngineApplicationModule),
    typeof(RenderEngineEntityFrameworkCoreModule),
    typeof(RenderEngineJsonFileRepositoryModule),
    //=====lowcode-web=====//
    //RenderEngine
    typeof(RenderEngineModule),
    //ComponentParts
    typeof(DefaultComponentModule),
    typeof(ExtensionComponentModule),
    //ThemeParts
    typeof(AntBlazorThemeModule)
    )]
public class RenderEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

        //状态管理
        context.Services.AddScoped(typeof(ComponentState<>));
        context.Services.AddScoped(typeof(ComponentState<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        
    }

    private void ConfigureAutoApiControllers()
    {
        //动态API注册
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(RenderEngineApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        //动态API swagger 注册
        var env = services.GetHostingEnvironment();
        if (env.IsDevelopment())
        {
            services.AddAbpSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RenderEngine", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                }
            );
        }
    }
}
