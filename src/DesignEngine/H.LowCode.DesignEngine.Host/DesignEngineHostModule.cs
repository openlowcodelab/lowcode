using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.DesignEngine.Application;
using H.LowCode.EntityFrameworkCore;
using H.LowCode.MyApp;
using H.LowCode.PartsDesignEngine;
using H.LowCode.Repository.JsonFile;
using H.LowCode.Workbench;
using H.Util.Blazor;
using Microsoft.OpenApi.Models;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace H.LowCode.DesignEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpSwashbuckleModule),
    //=====lowcode-server=====//
    typeof(DesignEngineApplicationModule),
    typeof(LowCodeEntityFrameworkCoreModule),
    typeof(MetaJsonFileRepositoryModule),
    //=====lowcode-web=====//
    //Workbench
    typeof(LowCodeWorkbenchModule),
    //DesignEngine
    typeof(DesignEngineModule),
    typeof(MyAppModule),
    typeof(PartsDesignEngineModule),
    //ComponentParts
    typeof(DefaultComponentModule),
    typeof(ExtensionComponentModule)
    )]
public class DesignEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

        //状态管理
        context.Services.AddScoped(typeof(ComponentState<>));
        context.Services.AddScoped(typeof(ComponentState<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(true));
    }

    private void ConfigureAutoApiControllers()
    {
        //动态API注册
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(DesignEngineApplicationModule).Assembly);
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
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DesignEngine", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                }
            );
        }
    }
}
