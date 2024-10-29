using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Domain;

public class LowCodeDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IAppDomainService, AppDomainService>();
        context.Services.AddScoped<IMenuDomainService, MenuDomainService>();
        context.Services.AddScoped<IPageDomainService, PageDomainService>();
        context.Services.AddScoped<IDataSourceDomainService, DataSourceDomainService>();

        context.Services.AddScoped<IFormDataDomainService, FormDataDomainService>();
        context.Services.AddScoped<ITableDataDomainService, TableDataDomainService>();
    }
}
