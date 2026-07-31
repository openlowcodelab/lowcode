using Avalonia.Controls;
using Avalonia.Input;

namespace H.AppLab.Desktop.Host.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 无标题栏模式下，顶部 Logo 区域兼作窗口拖拽区
    /// </summary>
    private void OnDragAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
}
