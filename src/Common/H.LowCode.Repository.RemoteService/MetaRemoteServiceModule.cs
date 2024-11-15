using H.LowCode.Domain;
using H.LowCode.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Repository.RemoteService;

[DependsOn(typeof(LowCodeDomainModule))]
public class MetaRemoteServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppRepository, AppRemoteServiceRepository>();
        context.Services.AddScoped<IMenuRepository, MenuRemoteServiceRepository>();
        context.Services.AddScoped<IPageRepository, PageRemoteServiceRepository>();
        context.Services.AddScoped<IDataSourceRepository, DataSourceRemoteServiceRepository>();
    }
}
