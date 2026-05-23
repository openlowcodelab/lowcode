using H.Agent.Application.Contracts;
using H.Agent.Application.Workers;
using H.Agent.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.Agent.Application;

[DependsOn(
    typeof(AgentApplicationContractsModule),
    typeof(AgentEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule)
)]
public class AgentApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AgentApplicationModule>();
        });


        // 注册 Agent 相关服务
        ConfigureAgentServices(context);
    }

    private void ConfigureAgentServices(ServiceConfigurationContext context)
    {
        // 注册 LLM 服务
        context.Services.AddScoped<LLMProviderFactory>();

        // 注册 Agent 工厂
        context.Services.AddTransient<AgentFactory>();

        // 注册定时任务 Worker
        context.Services.AddHostedService<ScheduledTaskWorker>();
    }
}
