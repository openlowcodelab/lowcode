using H.Workbench.Application.Workers;
using H.Workbench.Core;
using H.Workbench.Core.Mcp;
using H.Workbench.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.Workbench.Application;

[DependsOn(
    typeof(WorkbenchEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule),
    typeof(WorkbenchCoreModule)
)]
public class WorkbenchApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<WorkbenchApplicationModule>();
        });

        // 注册 Workbench 相关服务
        ConfigureWorkbenchServices(context);
    }

    private void ConfigureWorkbenchServices(ServiceConfigurationContext context)
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

        // 注册启动种子数据 Worker
        context.Services.AddHostedService<WorkbenchDataSeedWorker>();
    }
}
