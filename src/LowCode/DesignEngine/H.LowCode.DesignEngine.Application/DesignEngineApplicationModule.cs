using H.LowCode.Application;
using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace H.LowCode.DesignEngine.Application;

[DependsOn(
    typeof(LowCodeApplicationModule),
    typeof(DesignEngineDomainModule)
)]
public class DesignEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<List<SiteOption>>(configuration.GetSection(SiteOption.SectionName));

        // 从 DesignEngineApplicationContractsModule 迁移: 临时解决 ABP ValidationInterceptor 反射 MethodInfo 引发的异常
        Configure<AbpValidationOptions>(options =>
        {
            options.IgnoredTypes.Add(typeof(MethodBase));
            options.IgnoredTypes.Add(typeof(MethodInfo));
        });
    }
}
