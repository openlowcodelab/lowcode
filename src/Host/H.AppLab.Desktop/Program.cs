using Avalonia;

namespace H.AppLab.Desktop;

/// <summary>
/// AppLab 桌面宿主入口（Avalonia）：所有客户端应用的统一外壳
/// </summary>
internal static class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
