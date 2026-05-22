using H.Agent.Application.Contracts;
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
        context.Services.AddHttpContextAccessor();
        
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AgentApplicationModule>();
        });
        
        // 注册 Agent 相关服务
        ConfigureAgentServices(context);
    }
    
    private void ConfigureAgentServices(ServiceConfigurationContext context)
    {
        // 注册内存会话存储
        context.Services.AddSingleton<AgentSessionStore>();
        
        // 注册 LLM 服务
        context.Services.AddScoped<ILLMConfigAppService, LLMConfigAppService>();
        context.Services.AddScoped<LLMProviderFactory>();
        
        // 注册 Agent 工厂
        context.Services.AddTransient<AgentFactory>();
    }
}
