using Hangfire;
using Hangfire.Storage;
using H.Testing.Application.Contracts;
using H.Testing.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.Modularity;
using Volo.Abp;

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

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        CleanupLegacyScheduleJobs(context);
    }

    /// <summary>
    /// 定时任务功能已并入测试计划（作业前缀改为 testing-plan:），
    /// 清理 Hangfire 持久化存储中遗留的 testing-schedule:* 周期作业，避免其持续触发失败
    /// </summary>
    private static void CleanupLegacyScheduleJobs(ApplicationInitializationContext context)
    {
        var logger = context.ServiceProvider.GetRequiredService<ILogger<TestingApplicationModule>>();

        try
        {
            var legacyIds = JobStorage.Current.GetConnection().GetRecurringJobs()
                .Where(j => j.Id.StartsWith("testing-schedule:"))
                .Select(j => j.Id)
                .ToList();

            foreach (var id in legacyIds)
            {
                RecurringJob.RemoveIfExists(id);
            }

            if (legacyIds.Count > 0)
            {
                logger.LogInformation("已清理 {Count} 个遗留的定时任务周期作业（testing-schedule:*）", legacyIds.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "清理遗留定时任务周期作业失败，不影响应用启动");
        }
    }
}
