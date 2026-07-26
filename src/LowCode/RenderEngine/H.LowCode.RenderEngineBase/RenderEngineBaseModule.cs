using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// RenderEngineBase 服务注册扩展
/// </summary>
public static class RenderEngineBaseModule
{
    public static IServiceCollection AddRenderEngineBase(this IServiceCollection services)
    {
        // 注册 List 数据操作管理器（使用 Singleton 确保 WebAssembly 中状态保持）
        services.AddSingleton<ListDataOperationManager>();
        return services;
    }
}
