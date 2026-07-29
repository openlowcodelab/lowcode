using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using H.Assistant.UI.ViewModels;

namespace H.Assistant.UI.Views;

public partial class ChatView : UserControl
{
    /// <summary>结果标签颜色：错误红 / 成功绿</summary>
    public static readonly IValueConverter ResultLabelBrushConverter =
        new FuncValueConverter<bool, IBrush>(isError =>
            new SolidColorBrush(Color.Parse(isError ? "#ff4d4f" : "#52c41a")));

    private ChatViewModel? _viewModel;

    public ChatView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (_viewModel != null)
            {
                _viewModel.ScrollToBottomRequested -= ScrollToBottom;
            }
            _viewModel = DataContext as ChatViewModel;
            if (_viewModel != null)
            {
                _viewModel.ScrollToBottomRequested += ScrollToBottom;
            }
        };
    }

    private void ScrollToBottom()
    {
        Dispatcher.UIThread.Post(() =>
        {
            this.FindControl<ScrollViewer>("MessagesScroll")?.ScrollToEnd();
        }, DispatcherPriority.Background);
    }
}
