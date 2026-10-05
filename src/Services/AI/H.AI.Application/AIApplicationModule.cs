using H.AI.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.AI.Application;

[DependsOn(
    typeof(AIEntityFrameworkCoreModule),
    typeof(AbpAutoMapperModule)
)]
public class AIApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AIApplicationModule>();
        });

        context.Services.AddScoped<LLMProviderFactory>();
    }
}
