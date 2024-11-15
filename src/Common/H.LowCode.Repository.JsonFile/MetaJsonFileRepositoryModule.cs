using H.LowCode.Configuration;
using H.LowCode.Domain;
using H.LowCode.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Repository.JsonFile;

[DependsOn(typeof(LowCodeDomainModule))]
public class MetaJsonFileRepositoryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
