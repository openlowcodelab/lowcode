using H.LowCode.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngineBase;

public class RenderEngineBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册 List 数据操作管理器（使用 Singleton 确保 WebAssembly 中状态保持）
        context.Services.AddSingleton<ListDataOperationManager>();
    }
}
