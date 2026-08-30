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

    private void OnAiOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        // 点击遮罩关闭（生成中由 CloseAiGenerate 内部拦截）
        (DataContext as TasksViewModel)?.CloseAiGenerateCommand.Execute(null);
    }

    private void OnAiDescriptionKeyDown(object? sender, KeyEventArgs e)
    {
        // 多行输入框普通 Enter 为换行，用 Ctrl+Enter 提交
        if (e.Key == Key.Enter && e.KeyModifiers.HasFlag(KeyModifiers.Control)
            && DataContext is TasksViewModel vm)
        {
            vm.GenerateTaskFromAiCommand.Execute(null);
        }
    }
}
