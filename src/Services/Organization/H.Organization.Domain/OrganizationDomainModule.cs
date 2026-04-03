using Volo.Abp.Modularity;

namespace H.Organization.Domain;

public class OrganizationDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Domain 层主要是实体定义，通常不需要注册服务
    }
}
