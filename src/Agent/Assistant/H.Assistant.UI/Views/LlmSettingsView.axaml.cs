using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

public partial class LlmSettingsView : UserControl
{
    public LlmSettingsView()
    {
        InitializeComponent();
    }

    private void OnOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LlmSettingsViewModel)?.CloseDialogCommand.Execute(null);
    }
}
