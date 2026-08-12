using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using H.AppLab.Desktop.ViewModels;
using H.AppLab.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace H.AppLab.Desktop;

public partial class App : global::Avalonia.Application
{
    /// <summary>
    /// 全局服务容器（宿主与全部插件应用共用）
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = HostServices.Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
