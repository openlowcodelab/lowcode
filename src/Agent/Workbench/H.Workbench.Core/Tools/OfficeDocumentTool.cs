using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using DocumentFormat.OpenXml.Wordprocessing;
using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace H.Workbench.Core.Tools;

/// <summary>
/// Word 文档技能：把员工已经会写的结构化内容落成真正的 .docx，而不是"一段看起来像报告的聊天文本"。
/// 内容走块（block）协议而非自由 HTML：员工只需要给标题/段落/列表/表格，格式由服务端拼装。
/// 写完立刻用 OpenXmlValidator 校验并把问题计数回传——生成打不开的文件比不生成更糟。
/// </summary>
public class OfficeDocumentTool
{
    private const int MaxBlocks = 400;
    private const int MaxTextChars = 200000;

    private static readonly JsonSerializerOptions ReadJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<OfficeDocumentTool> _logger;

    public OfficeDocumentTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<OfficeDocumentTool> logger)
    {
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("生成 Word 文档（.docx）写入服务端工作目录，内容用块（block）协议描述。参数：fileName（.docx 结尾的相对路径）, blocksJson（块数组 JSON）, repo（可选，留空则写入工作目录的 outputs 文件夹）。块类型：{\"type\":\"heading\",\"text\":\"标题\",\"level\":1}、{\"type\":\"paragraph\",\"text\":\"正文\"}、{\"type\":\"list\",\"items\":[\"a\",\"b\"],\"ordered\":false}、{\"type\":\"table\",\"headers\":[\"列A\",\"列B\"],\"rows\":[[\"1\",\"2\"]]}、{\"type\":\"quote\",\"text\":\"引用\"}、{\"type\":\"code\",\"text\":\"等宽文本\"}、{\"type\":\"pagebreak\"}。")]
    public async Task<string> OfficeWriteDocumentAsync(
        [Description("文档相对路径，必须以 .docx 结尾，例如 report/周报.docx")] string fileName,
        [Description("内容块数组的 JSON 字符串")] string blocksJson,
        [Description("已克隆的仓库目录名或地址；留空则写入工作目录 outputs 下")] string? repo = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !fileName.Trim().EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail("fileName 必须以 .docx 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, fileName.Trim(), out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        List<DocumentBlock> blocks;
        try
        {
            blocks = JsonSerializer.Deserialize<List<DocumentBlock>>(blocksJson, ReadJson) ?? [];
        }
        catch (JsonException ex)
        {
            return ToolEnvelope.Fail($"blocksJson 不是合法 JSON 数组: {ex.Message}");
        }

        if (blocks.Count == 0)
        {
            return ToolEnvelope.Fail("blocksJson 至少要包含一个内容块");
        }

        if (blocks.Count > MaxBlocks)
        {
            return ToolEnvelope.Fail($"内容块数量超过上限 {MaxBlocks}");
        }

        var totalChars = blocks.Sum(b => (b.Text?.Length ?? 0) + (b.Items?.Sum(i => i?.Length ?? 0) ?? 0)
                                        + (b.Headers?.Sum(h => h?.Length ?? 0) ?? 0)
                                        + (b.Rows?.Sum(r => r?.Sum(c => c?.Length ?? 0) ?? 0) ?? 0));
        if (totalChars > MaxTextChars)
        {
            return ToolEnvelope.Fail($"内容总字数超过上限 {MaxTextChars}");
        }

        var dir = Path.GetDirectoryName(fullPath!)!;
        Directory.CreateDirectory(dir);

        int paragraphCount;
        int tableCount;
        int validationErrors;
        string? firstValidationError = null;

        using (await _locks.AcquireAsync(dir, cancellationToken))
        {
            using (var doc = WordprocessingDocument.Create(fullPath!, WordprocessingDocumentType.Document))
            {
                var mainPart = doc.AddMainDocumentPart();
                var body = new Body();
                mainPart.Document = new Document(body);

                paragraphCount = 0;
                tableCount = 0;
                foreach (var block in blocks)
                {
                    foreach (var element in BuildBlock(block))
                    {
                        body.Append(element);
                        if (element is Paragraph) paragraphCount++;
                        else if (element is Table) tableCount++;
                    }
                }

                body.Append(new SectionProperties(
                    new PageSize
                    {
                        Width = 11906u,
                        Height = 16838u
                    },
                    new PageMargin
                    {
                        Top = 1440,
                        Right = 1440,
                        Bottom = 1440,
                        Left = 1440,
                        Header = 720,
                        Footer = 720,
                        Gutter = 0
                    }));

                mainPart.Document.Save();

                var validator = new OpenXmlValidator();
                var errors = validator.Validate(doc).ToList();
                validationErrors = errors.Count;
                firstValidationError = errors.Count > 0 ? errors[0].Description : null;
            }

            // 校验不通过就删掉半成品：留下一个打不开的 docx 会让员工声称已交付
            if (validationErrors > 0)
            {
                try { File.Delete(fullPath!); } catch (IOException) { /* 留日志即可 */ }
            }
        }

        if (validationErrors > 0)
        {
            _logger.LogWarning("生成的 docx 未通过 OpenXml 校验: {File}, {Count} 个错误: {First}",
                fullPath, validationErrors, firstValidationError);
            return ToolEnvelope.Fail($"生成的文档结构校验未通过（{validationErrors} 个问题）：{firstValidationError}");
        }

        var relative = Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/');
        return ToolEnvelope.Ok(new
        {
            file = relative,
            format = "docx",
            sizeBytes = new FileInfo(fullPath!).Length,
            blocks = blocks.Count,
            paragraphs = paragraphCount,
            tables = tableCount,
            note = "文档为二进制产物，用 OfficeReadDocumentAsync 可回读文本核对"
        });
    }

    [Description("回读 .docx 正文文本（用于核对刚生成的文档或读取用户提供的文档）。参数：fileName（相对路径）, repo（可选）, maxChars（默认 4000）。表格按竖线分行返回。")]
    public async Task<string> OfficeReadDocumentAsync(
        [Description("文档相对路径，必须以 .docx 结尾")] string fileName,
        [Description("已克隆的仓库目录名或地址；留空则在工作目录 outputs 下查找")] string? repo = null,
        [Description("最多回读字符数")] int maxChars = 4000,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !fileName.Trim().EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            return ToolEnvelope.Fail("fileName 必须以 .docx 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, fileName.Trim(), out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        if (!File.Exists(fullPath!))
        {
            return ToolEnvelope.Fail($"文件不存在: {fileName}");
        }

        var limit = Math.Clamp(maxChars > 0 ? maxChars : 4000, 200, 20000);
        var sb = new StringBuilder();
        var paragraphCount = 0;
        var tableCount = 0;
        var truncated = false;

        using (await _locks.AcquireAsync(Path.GetDirectoryName(fullPath!)!, cancellationToken))
        {
            using var doc = WordprocessingDocument.Open(fullPath!, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body is null) return ToolEnvelope.Fail("文档没有正文内容");

            foreach (var child in body.ChildElements)
            {
                if (child is Paragraph p)
                {
                    var text = p.InnerText;
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    paragraphCount++;
                    var outline = p.Elements<ParagraphProperties>().FirstOrDefault()
                        ?.Elements<OutlineLevel>().FirstOrDefault()?.Val;
                    var rank = outline is null ? 0 : (int)outline.Value + 1;
                    var prefix = rank == 0 ? "" : new string('#', Math.Min(6, rank)) + " ";
                    if (!Append(sb, prefix + text.Trim() + Environment.NewLine, limit))
                    {
                        truncated = true;
                        break;
                    }
                }
                else if (child is Table t)
                {
                    tableCount++;
                    foreach (var row in t.Elements<TableRow>())
                    {
                        var line = string.Join(" | ", row.Elements<TableCell>().Select(c => c.InnerText.Trim()));
                        if (line.Length == 0) continue;
                        if (!Append(sb, line + Environment.NewLine, limit))
                        {
                            truncated = true;
                            break;
                        }
                    }

                    if (truncated) break;
                }
            }
        }

        var relativeRead = Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/');

        if (sb.Length == 0)
        {
            return ToolEnvelope.Ok(new { file = relativeRead, text = "", paragraphs = 0, tables = tableCount, note = "文档为空" });
        }

        return ToolEnvelope.Ok(new
        {
            file = relativeRead,
            format = "docx",
            text = sb.ToString(),
            paragraphs = paragraphCount,
            tables = tableCount,
            truncated
        });
    }

    private static bool Append(StringBuilder sb, string line, int limit)
    {
        if (sb.Length + line.Length > limit) return false;
        sb.Append(line);
        return true;
    }

    private static IEnumerable<OpenXmlElement> BuildBlock(DocumentBlock block)
    {
        var type = (block.Type ?? "paragraph").Trim().ToLowerInvariant();

        switch (type)
        {
            case "heading":
            {
                var level = Math.Clamp(block.Level ?? 1, 1, 4);
                yield return new Paragraph(new Run(MakeText(block.Text)) { RunProperties = HeadingProps(level) })
                {
                    ParagraphProperties = new ParagraphProperties(
                        new KeepNext(),
                        new SpacingBetweenLines { Before = level == 1 ? "240" : "160", After = "80" },
                        OutlineLevelFor(level))
                };
                break;
            }
            case "list":
            {
                var items = block.Items ?? [];
                var ordered = block.Ordered ?? false;
                for (var i = 0; i < items.Count; i++)
                {
                    var marker = ordered ? $"{i + 1}. " : "• ";
                    yield return new Paragraph(new Run(MakeText(marker + items[i])))
                    {
                        ParagraphProperties = new ParagraphProperties(
                            new Indentation
                            {
                                Left = (720 + (block.Nested ?? 0) * 360).ToString(),
                                Hanging = "360"
                            })
                    };
                }
                break;
            }
            case "table":
            {
                var headers = block.Headers ?? [];
                var rows = block.Rows ?? [];
                var columns = Math.Max(headers.Count, rows.Count == 0 ? 0 : rows.Max(r => r?.Count ?? 0));
                if (columns == 0)
                {
                    yield return new Paragraph(new Run(MakeText("（空表格）")));
                    break;
                }

                var columnWidth = Math.Max(600, 9000 / columns).ToString();
                var table = new Table(
                    new TableProperties(
                        new TableWidth { Width = "5000", Type = TableWidthUnitValues.Pct },
                        new TableBorders(
                            new TopBorder { Val = BorderValues.Single, Size = 4, Color = "999999" },
                            new LeftBorder { Val = BorderValues.Single, Size = 4, Color = "999999" },
                            new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "999999" },
                            new RightBorder { Val = BorderValues.Single, Size = 4, Color = "999999" },
                            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4, Color = "999999" },
                            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4, Color = "999999" })));

                var grid = new TableGrid();
                for (var c = 0; c < columns; c++)
                {
                    grid.Append(new GridColumn { Width = columnWidth });
                }

                table.Append(grid);

                if (headers.Count > 0)
                {
                    var headerRow = new TableRow();
                    for (var c = 0; c < columns; c++)
                    {
                        var text = c < headers.Count ? headers[c] : "";
                        headerRow.Append(new TableCell(
                            new Paragraph(new Run(MakeText(text)) { RunProperties = new RunProperties(Bold()) })));
                    }

                    table.Append(headerRow);
                }

                foreach (var cells in rows)
                {
                    var row = new TableRow();
                    var count = cells?.Count ?? 0;
                    for (var c = 0; c < columns; c++)
                    {
                        var text = c < count ? cells![c] : "";
                        row.Append(new TableCell(new Paragraph(new Run(MakeText(text)))));
                    }

                    table.Append(row);
                }

                // 表格后补一个空段落，否则 Word 会把下一个块并进表格
                yield return table;
                yield return new Paragraph(new Run(new Text("")));
                break;
            }
            case "quote":
            {
                yield return new Paragraph(new Run(MakeText(block.Text)) { RunProperties = new RunProperties(Italic()) })
                {
                    ParagraphProperties = new ParagraphProperties(
                        new Shading { Val = ShadingPatternValues.Clear, Fill = "F7F7F7" },
                        new Indentation { Left = "480" })
                };
                break;
            }
            case "code":
            {
                foreach (var line in (block.Text ?? "").Replace("\r\n", "\n").Split('\n'))
                {
                    yield return new Paragraph(new Run(MakeText(line)) { RunProperties = Monospace() })
                    {
                        ParagraphProperties = new ParagraphProperties(new Indentation { Left = "240" })
                    };
                }
                break;
            }
            case "pagebreak":
            {
                yield return new Paragraph(new Run(new Break { Type = BreakValues.Page }));
                break;
            }
            default:
            {
                yield return new Paragraph(new Run(MakeText(block.Text)))
                {
                    ParagraphProperties = new ParagraphProperties(new SpacingBetweenLines { After = "120" })
                };
                break;
            }
        }
    }

    private static Text MakeText(string? value) => new(value ?? "") { Space = SpaceProcessingModeValues.Preserve };

    /// <summary>
    /// rPr 的子元素顺序由 schema 固定（rFonts → b → i → color → sz），
    /// 顺序错了 OpenXmlValidator 会直接判文档非法
    /// </summary>
    private static RunProperties HeadingProps(int level) => level switch
    {
        1 => new RunProperties(Bold(), Color("1F3864"), FontSize("36")),
        2 => new RunProperties(Bold(), Color("2E5496"), FontSize("30")),
        3 => new RunProperties(Bold(), Color("2E5496"), FontSize("26")),
        _ => new RunProperties(Bold(), FontSize("24"))
    };

    private static Bold Bold() => new();
    private static Italic Italic() => new();
    private static FontSize FontSize(string halfPoints) => new() { Val = halfPoints };
    private static Color Color(string hex) => new() { Val = hex };

    /// <summary>
    /// 标题写 outlineLvl：Word 的导航窗格与目录认这个，光靠加粗字号不算层级
    /// </summary>
    private static OutlineLevel OutlineLevelFor(int level) => new() { Val = level - 1 };

    private static RunProperties Monospace() => new(
        new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", EastAsia = "Consolas" },
        FontSize("20"));

    private sealed class DocumentBlock
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
        public int? Level { get; set; }
        public List<string>? Items { get; set; }
        public bool? Ordered { get; set; }
        public int? Nested { get; set; }
        public List<string>? Headers { get; set; }
        public List<List<string>?>? Rows { get; set; }
    }
}
