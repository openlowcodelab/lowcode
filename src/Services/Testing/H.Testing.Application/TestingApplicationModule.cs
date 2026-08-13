using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Testing.Application;

/// <summary>
/// Testing 应用模块
/// </summary>
[DependsOn(
    typeof(TestingEntityFrameworkCoreModule)
)]
public class TestingApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ITestExecutionEventNotifier, TestExecutionEventNotifier>();

        // 测试执行引擎直接注入 HttpClient，通过 IHttpClientFactory 提供解析
        context.Services.AddHttpClient();
        context.Services.AddTransient<HttpClient>(sp =>
            sp.GetRequiredService<System.Net.Http.IHttpClientFactory>().CreateClient());
    }
}
