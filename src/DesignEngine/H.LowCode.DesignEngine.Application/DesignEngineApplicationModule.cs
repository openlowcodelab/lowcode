using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Application;

[DependsOn(typeof(LowCodeDomainModule))]
public class DesignEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppApplicationService, AppApplicationService>();
        context.Services.AddScoped<IMenuAppService, MenuAppService>();
        context.Services.AddScoped<IPageAppService, PageAppService>();
        context.Services.AddScoped<IDataSourceAppService, DataSourceAppService>();
    }
}
