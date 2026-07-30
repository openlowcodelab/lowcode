using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.Desktop.ViewModels;

namespace H.Assistant.Desktop.Views;

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
