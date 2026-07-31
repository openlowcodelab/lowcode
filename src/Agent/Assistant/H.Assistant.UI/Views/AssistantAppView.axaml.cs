using Avalonia.Controls;

namespace H.Assistant.UI.Views;

/// <summary>
/// 助手应用根视图（以插件应用形式嵌入 H.AppLab.Desktop.Host 宿主外壳）
/// </summary>
public partial class AssistantAppView : UserControl
{
    public AssistantAppView()
    {
        InitializeComponent();
    }
}
