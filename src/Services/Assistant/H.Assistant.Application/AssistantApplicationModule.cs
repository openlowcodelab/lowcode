using H.Assistant.Application.Contracts;
using H.Assistant.Application.Workers;
using H.Assistant.EntityFrameworkCore;
using H.Assistant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
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
