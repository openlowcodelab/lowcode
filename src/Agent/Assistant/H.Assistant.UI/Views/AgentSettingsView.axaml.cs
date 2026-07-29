using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

public partial class AgentSettingsView : UserControl
{
    public AgentSettingsView()
    {
        InitializeComponent();
    }

    private void OnOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as AgentSettingsViewModel)?.CloseDialogCommand.Execute(null);
    }
}
