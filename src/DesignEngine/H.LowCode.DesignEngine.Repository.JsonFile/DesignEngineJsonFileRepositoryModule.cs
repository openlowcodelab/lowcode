using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain;
using H.LowCode.DesignEngine.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.Repository.JsonFile;

[DependsOn(typeof(DesignEngineDomainModule))]
public class DesignEngineJsonFileRepositoryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppRepository, AppFileRepository>();
        context.Services.AddScoped<IMenuRepository, MenuFileRepository>();
        context.Services.AddScoped<IPageRepository, PageFileRepository>();
        context.Services.AddScoped<IDataSourceRepository, DataSourceFileRepository>();

        context.Services.AddScoped<IComponentLibraryRepository, ComponentLibraryRepository>();
        context.Services.AddScoped<IComponentPartsRepository, ComponentPartsRepository>();

        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
