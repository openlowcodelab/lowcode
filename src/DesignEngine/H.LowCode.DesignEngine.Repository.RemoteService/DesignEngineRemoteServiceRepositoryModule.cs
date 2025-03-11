using H.LowCode.DesignEngine.Domain;
using H.LowCode.DesignEngine.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Repository.RemoteService;

[DependsOn(typeof(DesignEngineDomainModule))]
public class DesignEngineRemoteServiceRepositoryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppRepository, AppRemoteServiceRepository>();
        context.Services.AddScoped<IMenuRepository, MenuRemoteServiceRepository>();
        context.Services.AddScoped<IPageRepository, PageRemoteServiceRepository>();
        context.Services.AddScoped<IDataSourceRepository, DataSourceRemoteServiceRepository>();
    }
}
