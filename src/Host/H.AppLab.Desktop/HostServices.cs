using H.AppLab.Desktop.Services;
using H.AppLab.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Desktop;

/// <summary>
/// 宿主服务注册：统一加载配置并汇集所有插件应用（<see cref="IDesktopApp"/>）的服务
/// </summary>
public static class HostServices
{
    /// <summary>
    /// 插件应用注册表：新应用在此追加即可接入宿主外壳
    /// </summary>
    private static IReadOnlyList<IDesktopApp> CreateApps() =>
    [
        new AssistantApp()
    ];

    public static IServiceProvider Build()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // 宿主基础服务
        services.AddSingleton<CategoryService>();
        services.AddSingleton<ToastService>();

        // 插件应用：注册应用实例及各自的服务
        foreach (var app in CreateApps())
        {
            services.AddSingleton(app);
            app.ConfigureServices(services, configuration);
        }

        // 宿主 ViewModel
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}
