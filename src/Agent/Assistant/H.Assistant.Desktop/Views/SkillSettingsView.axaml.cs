using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.Desktop.ViewModels;

namespace H.Assistant.Desktop.Views;

public partial class SkillSettingsView : UserControl
{
    public SkillSettingsView()
    {
        InitializeComponent();
    }

    private void OnOverlayPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as SkillSettingsViewModel)?.CloseDialogCommand.Execute(null);
    }
}
