using H.LowCode.DesignEngine.Application;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Host.Client;
using H.LowCode.DesignEngine.HttpApi;
using H.LowCode.EntityFrameworkCore;
using H.LowCode.Repository.JsonFile;
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
    //server
    typeof(DesignEngineHttpApiModule),
    typeof(DesignEngineApplicationModule),
    typeof(LowCodeEntityFrameworkCoreModule),
    typeof(MetaJsonFileRepositoryModule),
    //web
    typeof(DesignEngineHostClientModule)
    )]
public class DesignEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);
    }

    private void ConfigureAutoApiControllers()
    {
        //动态API扫描
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
                    var deliveryFile = Path.Combine(AppContext.BaseDirectory,
                        $"{typeof(DesignEngineApplicationContractsModule).Namespace}.xml");
                    if (File.Exists(deliveryFile))
                        options.IncludeXmlComments(deliveryFile);

                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DesignEngine", Version = "v1" });
                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);
                    //options.HideAbpEndpoints();
                    //options.SchemaFilter<HideAbpSchemaFilter>();
                }
            );
        }
    }
}
