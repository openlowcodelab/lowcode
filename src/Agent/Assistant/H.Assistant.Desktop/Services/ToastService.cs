using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace H.Assistant.Desktop.Services;

/// <summary>
/// 全局 Toast 提示服务（对应 Web 端 ChatStateBus.showToast，3 秒后自动消失）
/// </summary>
public partial class ToastService : ObservableObject
{
    [ObservableProperty]
    private string message = string.Empty;

    [ObservableProperty]
    private bool isVisible;

    [ObservableProperty]
    private IBrush background = Brushes.Gray;

    private CancellationTokenSource? _hideCts;

    public void Show(string message, string type = "info")
    {
        Dispatcher.UIThread.Post(() =>
        {
            Message = message;
            Background = type switch
            {
                "success" => new SolidColorBrush(Color.Parse("#52c41a")),
                "warning" => new SolidColorBrush(Color.Parse("#faad14")),
                "error" => new SolidColorBrush(Color.Parse("#ff4d4f")),
                _ => new SolidColorBrush(Color.Parse("#595959"))
            };
            IsVisible = true;

            _hideCts?.Cancel();
            _hideCts = new CancellationTokenSource();
            var token = _hideCts.Token;
            _ = Task.Delay(3000, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                {
                    Dispatcher.UIThread.Post(() => IsVisible = false);
                }
            }, TaskScheduler.Default);
        });
    }
}
