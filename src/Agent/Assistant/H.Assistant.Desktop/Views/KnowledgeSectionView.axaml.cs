using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using H.Assistant.Desktop.ViewModels;

namespace H.Assistant.Desktop.Views;

public partial class KnowledgeSectionView : UserControl
{
    public KnowledgeSectionView()
    {
        InitializeComponent();
    }

    private void OnTreeSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is TreeView { SelectedItem: KnowledgeNodeItem node })
        {
            node.Section.SelectNodeCommand.Execute(node);
        }
    }

    private void OnTitleDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Control { DataContext: KnowledgeNodeItem node })
        {
            node.StartRenameCommand.Execute(null);
        }
    }

    private void OnRenameBoxAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // 重命名输入框出现时自动聚焦
        if (sender is TextBox box && box.IsVisible)
        {
            box.Focus();
            box.SelectAll();
        }
    }

    private void OnRenameKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not Control { DataContext: KnowledgeNodeItem node })
        {
            return;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            node.CommitRenameCommand.Execute(null);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            node.CancelRenameCommand.Execute(null);
        }
    }

    private void OnRenameLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: KnowledgeNodeItem node })
        {
            node.CommitRenameCommand.Execute(null);
        }
    }

    private KnowledgeSectionViewModel? Section => DataContext as KnowledgeSectionViewModel;

    private void OnPanelTitleDoubleTapped(object? sender, TappedEventArgs e)
    {
        Section?.StartTitleEditCommand.Execute(null);
    }

    private void OnPanelTitleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Section?.CommitTitleEditCommand.Execute(null);
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Section?.CancelTitleEditCommand.Execute(null);
        }
    }

    private void OnPanelTitleLostFocus(object? sender, RoutedEventArgs e)
    {
        if (Section is { IsEditingTitle: true })
        {
            Section.CommitTitleEditCommand.Execute(null);
        }
    }
}
