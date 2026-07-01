using H.Portal.Application.Contracts;
using Volo.Abp.Modularity;

namespace H.Portal.Application;

[DependsOn(
    typeof(PortalApplicationContractsModule)
)]
public class PortalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册应用服务
    }
}
