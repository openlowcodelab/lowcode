using Avalonia.Controls;
using Avalonia.Input;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

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
