using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace H.Assistant.UI.Controls;

/// <summary>
/// 轻量 Markdown 渲染控件（基于 Markdig AST），
/// 视觉风格对齐 Web 端 .markdown-content 样式
/// </summary>
public class MarkdownTextView : ContentControl
{
    public static readonly StyledProperty<string?> MarkdownProperty =
        AvaloniaProperty.Register<MarkdownTextView, string?>(nameof(Markdown));

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .UseEmojiAndSmiley()
        .Build();

    private static readonly IBrush TextBrush = new SolidColorBrush(Color.Parse("#262626"));
    private static readonly IBrush MutedBrush = new SolidColorBrush(Color.Parse("#595959"));
    private static readonly IBrush CodeBackground = new SolidColorBrush(Color.Parse("#f0f0f0"));
    private static readonly IBrush CodeBlockBackground = new SolidColorBrush(Color.Parse("#f5f5f5"));
    private static readonly IBrush BorderBrushColor = new SolidColorBrush(Color.Parse("#e8e8e8"));
    private static readonly IBrush QuoteBarBrush = new SolidColorBrush(Color.Parse("#a6a6a6"));
    private static readonly IBrush LinkBrush = new SolidColorBrush(Color.Parse("#1890ff"));
    private static readonly FontFamily CodeFontFamily = new("Consolas,Monaco,Courier New,monospace");

    public string? Markdown
    {
        get => GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    static MarkdownTextView()
    {
        MarkdownProperty.Changed.AddClassHandler<MarkdownTextView>((c, _) => c.Render());
    }

    private void Render()
    {
        if (string.IsNullOrEmpty(Markdown))
        {
            Content = null;
            return;
        }

        var document = Markdig.Markdown.Parse(Markdown, Pipeline);
        var panel = new StackPanel { Spacing = 6 };
        foreach (var block in document)
        {
            var control = RenderBlock(block);
            if (control != null)
            {
                panel.Children.Add(control);
            }
        }
        Content = panel;
    }

    private Control? RenderBlock(Block block)
    {
        switch (block)
        {
            case HeadingBlock heading:
                return RenderHeading(heading);
            case ParagraphBlock paragraph:
                return RenderParagraph(paragraph);
            case FencedCodeBlock fenced:
                return RenderCodeBlock(fenced.Lines.ToString());
            case CodeBlock code:
                return RenderCodeBlock(code.Lines.ToString());
            case QuoteBlock quote:
                return RenderQuote(quote);
            case ListBlock list:
                return RenderList(list);
            case ThematicBreakBlock:
                return new Border { Height = 1, Background = BorderBrushColor, Margin = new Thickness(0, 8) };
            case Table table:
                return RenderTable(table);
            default:
                return null;
        }
    }

    private Control RenderHeading(HeadingBlock heading)
    {
        var fontSize = heading.Level switch
        {
            1 => 24.0,
            2 => 21.0,
            3 => 17.5,
            _ => 14.0
        };
        var text = CreateTextBlock(fontSize, FontWeight.SemiBold);
        AppendInlines(text.Inlines!, heading.Inline);

        if (heading.Level <= 2)
        {
            return new Border
            {
                BorderBrush = BorderBrushColor,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 0, 0, 4),
                Margin = new Thickness(0, 6, 0, 2),
                Child = text
            };
        }

        text.Margin = new Thickness(0, 6, 0, 2);
        return text;
    }

    private Control RenderParagraph(ParagraphBlock paragraph)
    {
        var text = CreateTextBlock(14, FontWeight.Normal);
        AppendInlines(text.Inlines!, paragraph.Inline);
        return text;
    }

    private Control RenderCodeBlock(string code)
    {
        return new Border
        {
            Background = CodeBlockBackground,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 4),
            Child = new SelectableTextBlock
            {
                Text = code.TrimEnd('\n'),
                FontFamily = CodeFontFamily,
                FontSize = 12.5,
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap
            }
        };
    }

    private Control RenderQuote(QuoteBlock quote)
    {
        var panel = new StackPanel { Spacing = 4 };
        foreach (var child in quote)
        {
            var control = RenderBlock(child);
            if (control != null)
            {
                panel.Children.Add(control);
            }
        }

        return new Border
        {
            BorderBrush = QuoteBarBrush,
            BorderThickness = new Thickness(4, 0, 0, 0),
            Background = new SolidColorBrush(Color.Parse("#f7f7f7")),
            Padding = new Thickness(12, 6),
            Margin = new Thickness(0, 4),
            Child = panel
        };
    }

    private Control RenderList(ListBlock list)
    {
        var panel = new StackPanel { Spacing = 3, Margin = new Thickness(8, 2, 0, 2) };
        var index = 1;
        if (list.IsOrdered && int.TryParse(list.OrderedStart, out var start))
        {
            index = start;
        }

        foreach (var item in list.OfType<ListItemBlock>())
        {
            var marker = list.IsOrdered ? $"{index}." : "•";
            index++;

            var itemPanel = new StackPanel { Spacing = 3 };
            foreach (var child in item)
            {
                var control = RenderBlock(child);
                if (control != null)
                {
                    itemPanel.Children.Add(control);
                }
            }

            var row = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*") };
            var markerText = new TextBlock
            {
                Text = marker,
                Foreground = MutedBrush,
                FontSize = 14,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(markerText, 0);
            Grid.SetColumn(itemPanel, 1);
            row.Children.Add(markerText);
            row.Children.Add(itemPanel);
            panel.Children.Add(row);
        }
        return panel;
    }

    private Control RenderTable(Table table)
    {
        var grid = new Grid { Margin = new Thickness(0, 4) };
        var columnCount = table.ColumnDefinitions.Count > 0 ? table.ColumnDefinitions.Count : 1;
        for (var i = 0; i < columnCount; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        }

        var rowIndex = 0;
        foreach (var rowObj in table)
        {
            if (rowObj is not TableRow row)
            {
                continue;
            }

            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            var colIndex = 0;
            foreach (var cellObj in row)
            {
                if (cellObj is not TableCell cell || colIndex >= columnCount)
                {
                    continue;
                }

                var text = CreateTextBlock(13, row.IsHeader ? FontWeight.SemiBold : FontWeight.Normal);
                foreach (var cellBlock in cell)
                {
                    if (cellBlock is LeafBlock leaf)
                    {
                        AppendInlines(text.Inlines!, leaf.Inline);
                    }
                }

                var border = new Border
                {
                    BorderBrush = BorderBrushColor,
                    BorderThickness = new Thickness(0.5),
                    Background = row.IsHeader ? new SolidColorBrush(Color.Parse("#fafafa")) : Brushes.Transparent,
                    Padding = new Thickness(10, 6),
                    Child = text
                };
                Grid.SetRow(border, rowIndex);
                Grid.SetColumn(border, colIndex);
                grid.Children.Add(border);
                colIndex++;
            }
            rowIndex++;
        }
        return grid;
    }

    private static SelectableTextBlock CreateTextBlock(double fontSize, FontWeight weight)
    {
        // 注意：不要设置 LineSpacing，Avalonia 测量高度不含行距，
        // 会导致长文本实际渲染高度大于布局高度，滚动区底部内容无法滚动到
        return new SelectableTextBlock
        {
            FontSize = fontSize,
            FontWeight = weight,
            Foreground = TextBrush,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = fontSize * 1.6
        };
    }

    private void AppendInlines(InlineCollection target, ContainerInline? container,
        FontWeight? weight = null, FontStyle? style = null, bool isLink = false)
    {
        if (container == null)
        {
            return;
        }

        foreach (var inline in container)
        {
            switch (inline)
            {
                case LiteralInline literal:
                    target.Add(CreateRun(literal.Content.ToString(), weight, style, isLink));
                    break;
                case EmphasisInline emphasis:
                    var childWeight = emphasis.DelimiterCount >= 2 ? FontWeight.SemiBold : weight;
                    var childStyle = emphasis.DelimiterCount == 1 ? FontStyle.Italic : style;
                    AppendInlines(target, emphasis, childWeight, childStyle, isLink);
                    break;
                case CodeInline code:
                    target.Add(new Run(code.Content)
                    {
                        FontFamily = CodeFontFamily,
                        FontSize = 12.5,
                        Background = CodeBackground,
                        Foreground = new SolidColorBrush(Color.Parse("#d4380d"))
                    });
                    break;
                case LinkInline link when !link.IsImage:
                    AppendInlines(target, link, weight, style, isLink: true);
                    break;
                case LinkInline { IsImage: true } image:
                    target.Add(CreateRun($"[图片: {image.Url}]", weight, style, isLink));
                    break;
                case LineBreakInline:
                    target.Add(new LineBreak());
                    break;
                case HtmlInline html:
                    target.Add(CreateRun(html.Tag, weight, style, isLink));
                    break;
                case AutolinkInline autolink:
                    target.Add(CreateRun(autolink.Url, weight, style, isLink: true));
                    break;
                case ContainerInline childContainer:
                    AppendInlines(target, childContainer, weight, style, isLink);
                    break;
                default:
                    var fallback = inline.ToString();
                    if (!string.IsNullOrEmpty(fallback))
                    {
                        target.Add(CreateRun(fallback, weight, style, isLink));
                    }
                    break;
            }
        }
    }

    private static Run CreateRun(string? text, FontWeight? weight, FontStyle? style, bool isLink)
    {
        var run = new Run(text ?? string.Empty);
        if (weight.HasValue)
        {
            run.FontWeight = weight.Value;
        }
        if (style.HasValue)
        {
            run.FontStyle = style.Value;
        }
        if (isLink)
        {
            run.Foreground = LinkBrush;
        }
        return run;
    }
}
