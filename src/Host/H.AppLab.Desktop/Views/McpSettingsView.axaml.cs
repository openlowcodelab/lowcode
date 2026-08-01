using Avalonia.Controls;
using Avalonia.Input;
using H.AppLab.Desktop.ViewModels;

namespace H.AppLab.Desktop.Views;

public partial class McpSettingsView : UserControl
{
    public McpSettingsView()
    {
        InitializeComponent();
    }

    private void OnOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as McpSettingsViewModel)?.CloseDialogCommand.Execute(null);
    }
}
