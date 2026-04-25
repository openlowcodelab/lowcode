using H.AutoTest.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.AutoTest.Application;

/// <summary>
/// AutoTest 应用模块
/// </summary>
[DependsOn(typeof(AutoTestApplicationContractsModule))]
public class AutoTestApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 HttpClient
        context.Services.AddHttpClient();
        
    }
}
