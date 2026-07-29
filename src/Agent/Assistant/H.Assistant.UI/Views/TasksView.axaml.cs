using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

public partial class TasksView : UserControl
{
    public TasksView()
    {
        InitializeComponent();
    }

    private void OnOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        // 点击遮罩关闭对话框（与 Web 端一致）
        (DataContext as TasksViewModel)?.CloseTaskDialogCommand.Execute(null);
    }
}
