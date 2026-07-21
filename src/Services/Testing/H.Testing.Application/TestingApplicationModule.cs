using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.Testing.Application;

/// <summary>
/// Testing 应用模块
/// </summary>
[DependsOn(
    typeof(TestingApplicationContractsModule),
    typeof(TestingEntityFrameworkCoreModule)
)]
public class TestingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {

    }
}
