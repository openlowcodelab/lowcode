using H.LowCode.EntityFrameworkCore;
using H.LowCode.RenderEngine.Application;
using H.LowCode.RenderEngine.Host.Client;
using H.LowCode.RenderEngine.Host.Components;
using H.LowCode.RenderEngine.HttpApi;
using H.LowCode.Repository.JsonFile;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace H.LowCode.RenderEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreModule),
    typeof(AbpSwashbuckleModule),
    //server
    typeof(RenderEngineHttpApiModule),
    typeof(RenderEngineApplicationModule),
    typeof(LowCodeEntityFrameworkCoreModule),
    typeof(MetaJsonFileRepositoryModule),
    //web
    typeof(RenderEngineHostClientModule)
    )]
public class RenderEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureConventionalControllers();
        ConfigureSwaggerServices(context.Services);
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
