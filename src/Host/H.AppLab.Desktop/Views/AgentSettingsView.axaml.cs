using Avalonia.Controls;
using Avalonia.Input;
using H.AppLab.Desktop.ViewModels;

namespace H.AppLab.Desktop.Views;

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
