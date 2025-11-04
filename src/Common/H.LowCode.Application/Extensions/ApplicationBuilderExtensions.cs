using Microsoft.AspNetCore.Builder;

namespace H.LowCode.Application;

/// <summary>
/// ApplicationBuilder 扩展方法
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 使用应用上下文中间件
    /// 该中间件会在每个请求开始时自动解析并设置 AppId
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器</returns>
    public static IApplicationBuilder UseAppContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AppContextMiddleware>();
    }
}