using H.BackgroundTask.Application.Contracts;
using H.BackgroundTask.Application.Services;
using H.BackgroundTask.EntityFrameworkCore;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.BackgroundTask.Application;

[DependsOn(
    typeof(BackgroundTaskApplicationContractsModule),
    typeof(BackgroundTaskEntityFrameworkCoreModule)
)]
public class BackgroundTaskApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.AddHttpClient();

        // 领域/执行服务
        context.Services.AddScoped<IBackgroundJobExecutor, BackgroundJobExecutor>();
        context.Services.AddTransient<IJobScheduler, HangfireJobScheduler>();

        ConfigureHangfire(context);
    }

    /// <summary>
    /// 配置 Hangfire：以后台任务库作为作业存储，并启动内置作业服务器。
    /// Hangfire 会在首次启动时自动在该数据库创建 [HangFire] 架构下的作业表。
    /// </summary>
    private static void ConfigureHangfire(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("BackgroundTaskDb");

        context.Services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                  {
                      CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                      SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                      QueuePollInterval = TimeSpan.Zero,
                      UseRecommendedIsolationLevel = true,
                      DisableGlobalLocks = true
                  });
        });

        // 启动 Hangfire 作业服务器（作为 IHostedService 随宿主一起运行）
        context.Services.AddHangfireServer();
    }
}
