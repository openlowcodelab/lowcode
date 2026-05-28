using H.Assistant.Application.Contracts;
using H.Assistant.Application.Workers;
using H.Assistant.EntityFrameworkCore;
using H.Assistant.Core;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.AutoMapper;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace H.Assistant.Application;

[DependsOn(
    typeof(AssistantApplicationContractsModule),
    typeof(AssistantEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule)
)]
public class AssistantApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AssistantApplicationModule>();
        });

        // 注册 Assistant 相关服务
        ConfigureAssistantServices(context);
    }

    public override async void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // 使用 IDataSeeder 触发数据种子，由框架管理 DbContext 生命周期
        //var dataSeeder = context.ServiceProvider.GetRequiredService<IDataSeeder>();
        //await dataSeeder.SeedAsync();
    }

    private void ConfigureAssistantServices(ServiceConfigurationContext context)
    {
        // 注册 LLM 服务
        context.Services.AddScoped<LLMProviderFactory>();

        // 注册 Agent 工厂
        context.Services.AddTransient<AgentFactory>();

        // 注册定时任务 Worker
        context.Services.AddHostedService<ScheduledTaskWorker>();
    }
}
