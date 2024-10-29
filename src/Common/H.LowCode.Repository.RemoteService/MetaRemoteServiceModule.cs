using H.LowCode.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.Repository.RemoteService;

[DependsOn(typeof(LowCodeDomainModule))]
public class MetaRemoteServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        
    }
}
