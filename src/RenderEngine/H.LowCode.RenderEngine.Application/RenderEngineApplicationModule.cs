using H.LowCode.Configuration;
using H.LowCode.RenderEngine.Domain;
using H.LowCode.ComponentBase.Services;
using H.LowCode.RenderEngine.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application;

[DependsOn(
    //abp
    typeof(AbpAutoMapperModule),
    //lowcode
    typeof(RenderEngineDomainModule)
    )]
public class RenderEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<RenderEngineApplicationModule>();
        });

        // 注册渲染引擎专用的表格数据提供者，覆盖默认实现
        context.Services.AddTransient<ITableDataProvider, RenderEngineTableDataProvider>();
    }
}
