using H.LowCode.EntityFrameworkCore;
using H.LowCode.RenderEngine.Host.Client;
using H.LowCode.RenderEngine.Host.Components;
using H.LowCode.RenderEngine.HttpApi;
using H.LowCode.Repository.JsonFile;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.AspNetCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Host;

[DependsOn(
    //abp
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreModule),
    //server
    typeof(RenderEngineHttpApiModule),
    typeof(LowCodeEntityFrameworkCoreModule),
    typeof(MetaJsonFileRepositoryModule),
    //web
    typeof(RenderEngineHostClientModule)
    )]
public class RenderEngineHostModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        
    }
}
