using H.LowCode.RenderEngine.Domain;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Repository.RemoteService;

[DependsOn(typeof(RenderEngineDomainModule))]
public class RenderEngineRemoteServiceRepositoryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppRepository, AppRemoteServiceRepository>();
        context.Services.AddScoped<IMenuRepository, MenuRemoteServiceRepository>();
        context.Services.AddScoped<IPageRepository, PageRemoteServiceRepository>();
        context.Services.AddScoped<IDataSourceRepository, DataSourceRemoteServiceRepository>();
    }
}
