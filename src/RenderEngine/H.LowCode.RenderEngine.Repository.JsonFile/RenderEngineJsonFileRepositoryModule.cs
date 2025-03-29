using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Repository.JsonFile;

[DependsOn(typeof(RenderEngineDomainModule))]
public class RenderEngineJsonFileRepositoryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppRepository, AppFileRepository>();
        context.Services.AddScoped<IMenuRepository, MenuFileRepository>();
        context.Services.AddScoped<IPageRepository, PageFileRepository>();
        context.Services.AddScoped<IDataSourceRepository, DataSourceFileRepository>();

        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
