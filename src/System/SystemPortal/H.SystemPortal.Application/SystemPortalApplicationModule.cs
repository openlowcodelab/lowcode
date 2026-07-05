using H.SystemPortal.Application.Contracts;
using Volo.Abp.Modularity;

namespace H.SystemPortal.Application;

[DependsOn(
    typeof(SystemPortalApplicationContractsModule)
)]
public class SystemPortalApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册应用服务
    }
}
