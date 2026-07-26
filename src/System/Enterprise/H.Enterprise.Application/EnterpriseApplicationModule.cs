using H.Enterprise.Application.Contracts;
using H.Enterprise.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Enterprise.Application;

[DependsOn(
    typeof(EnterpriseEntityFrameworkCoreModule)
)]
public class EnterpriseApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // ApplicationService 通过 ABP 自动注册（ITransientDependency）
    }
}
