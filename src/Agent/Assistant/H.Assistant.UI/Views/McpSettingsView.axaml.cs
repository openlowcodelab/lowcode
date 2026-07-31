using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

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
