using Avalonia.Controls;
using Avalonia.Input;
using H.AppLab.Desktop.ViewModels;

namespace H.AppLab.Desktop.Views;

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

    private void OnNewCategoryKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is TasksViewModel vm)
        {
            vm.ConfirmAddCategoryCommand.Execute(null);
        }
    }
}
