using Volo.Abp.Modularity;

namespace H.Account.Domain;

public class AccountDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Domain 层主要是实体定义，通常不需要注册服务
        // 如果需要注册领域服务，可以在这里添加
    }
}
