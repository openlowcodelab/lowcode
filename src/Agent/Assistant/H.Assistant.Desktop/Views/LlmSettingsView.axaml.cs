using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.Desktop.ViewModels;

namespace H.Assistant.Desktop.Views;

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
