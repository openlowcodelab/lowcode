using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace H.AppLab.Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AddHandler(PointerPressedEvent, OnBlankAreaPointerPressed);
    }

    /// <summary>
    /// 无标题栏模式下，顶部 Logo 区域兼作窗口拖拽区
    /// </summary>
    private void OnDragAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            e.Handled = true;
            BeginMoveDrag(e);
        }
    }

    /// <summary>
    /// 页面空白处（非交互元素）按下并拖拽时移动窗口
    /// </summary>
    private void OnBlankAreaPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Handled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        if (IsInteractive(e.Source as Visual))
            return;
        BeginMoveDrag(e);
    }

    private static bool IsInteractive(Visual? element)
    {
        for (var v = element; v != null; v = v.GetVisualParent())
        {
            if (v is Button or TextBox or SelectableTextBlock or ComboBox
                or ListBox or TreeView or TabControl or MenuBase or Slider
                or ScrollBar or NumericUpDown)
                return true;
        }
        return false;
    }
}
