using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using H.Assistant.Desktop.ViewModels;
using H.Assistant.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace H.Assistant.Desktop;

public partial class App : global::Avalonia.Application
{
    /// <summary>
    /// 全局服务容器
    /// </summary>
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Services = ClientServices.Build();

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
