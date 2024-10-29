using H.LowCode.Configuration;
using H.LowCode.Domain;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.Application;

[DependsOn(typeof(LowCodeDomainModule))]
public class RenderEngineApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IRenderEngineAppService, RenderEngineAppService>();

        context.Services.AddScoped<IFormDataAppService, FormDataAppService>();
        context.Services.AddScoped<ITableDataAppService, TableDataAppService>();

        var configuration = context.Services.GetConfiguration();
        context.Services.Configure<MetaOption>(configuration.GetSection(MetaOption.SectionName));
    }
}
