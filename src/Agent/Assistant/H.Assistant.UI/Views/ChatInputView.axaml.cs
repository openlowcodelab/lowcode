using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

public partial class ChatInputView : UserControl
{
    public ChatInputView()
    {
        InitializeComponent();

        // Enter 发送、Shift+Enter 换行（与 Web 端一致）
        var inputBox = this.FindControl<TextBox>("InputBox");
        inputBox?.AddHandler(KeyDownEvent, OnInputKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            if (DataContext is ChatViewModel vm && vm.SendCommand.CanExecute(null))
            {
                vm.SendCommand.Execute(null);
            }
        }
    }
}
