using H.Assistant.Application.Workers;
using H.Assistant.Core;
using H.Assistant.Core.Mcp;
using H.Assistant.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.Assistant.Application;

[DependsOn(
    typeof(AssistantEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule),
    typeof(AssistantCoreModule)
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

        // 注册工具注册中心（单例，保持工具注册状态）
        context.Services.AddSingleton<IToolRegistry, ToolRegistry>();

        // 注册 MCP Client 管理器（单例，保持连接状态）
        context.Services.AddSingleton<McpClientManager>();

        // 注册 Agent 工厂
        context.Services.AddTransient<AgentFactory>();

        // 注册定时任务 Worker
        context.Services.AddHostedService<TaskWorker>();
    }
}
