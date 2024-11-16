using H.LowCode.ComponentBase;
using H.LowCode.ComponentParts.BasicComponents;
using H.LowCode.ComponentParts.ExtensionComponents;
using H.LowCode.EntityFrameworkCore;
using H.LowCode.RenderEngine.Application;
using H.LowCode.Repository.JsonFile;
using H.LowCode.ThemeParts.AntBlazor;
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
    typeof(LowCodeEntityFrameworkCoreModule),
    typeof(MetaJsonFileRepositoryModule),
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
        ConfigureConventionalControllers();
        ConfigureSwaggerServices(context.Services);

        //状态管理
        context.Services.AddScoped(typeof(ComponentStateWrapper<,>));

        //应用状态
        context.Services.AddSingleton(new LowCodeAppState(false));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        
    }

    private void ConfigureConventionalControllers()
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
