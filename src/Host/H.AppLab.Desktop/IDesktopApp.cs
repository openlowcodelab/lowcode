using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Desktop;

/// <summary>
/// 桌面应用插件契约：独立客户端以“应用”的形式接入 H.AppLab.Desktop 宿主外壳。
/// 宿主启动时统一调用 <see cref="ConfigureServices"/> 注册服务，
/// 应用首次激活时调用 <see cref="CreateView"/> 创建根视图（实例由宿主缓存复用）。
/// </summary>
public interface IDesktopApp
{
    /// <summary>应用唯一标识（如 assistant）</summary>
    string Id { get; }

    /// <summary>应用显示名称（用于菜单与应用中心）</summary>
    string Name { get; }

    /// <summary>应用图标（emoji 字符）</summary>
    string Icon { get; }

    /// <summary>应用描述（应用中心展示）</summary>
    string Description => string.Empty;

    /// <summary>注册应用自身的服务（配置由宿主统一提供）</summary>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>创建应用根视图</summary>
    Control CreateView(IServiceProvider services);
}
