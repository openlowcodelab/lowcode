using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;
using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using SpreadsheetWorkbook = DocumentFormat.OpenXml.Spreadsheet.Workbook;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 表格技能：员工交付"报表"应该是真正的 xlsx/csv，而不是一段贴在对齐混乱的聊天里。
/// 读侧刻意限制返回体积——工具结果超过 4000 字会被轨迹截断成半截 JSON，
/// 那比返回少几行更糟（模型看到的是断掉的证据）。
/// </summary>
public class SpreadsheetTool
{
    private const int MaxSheets = 20;
    private const int MaxRowsPerSheet = 5000;
    private const int MaxColumns = 100;

    /// <summary>
    /// 回读体积上限：ToolExecutor 在 4000 字截断，留 700 字余量给信封与提示字段
    /// </summary>
    private const int ReadPayloadBudget = 3300;

    private static readonly JsonSerializerOptions ReadJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<SpreadsheetTool> _logger;

    public SpreadsheetTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<SpreadsheetTool> logger)
    {
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("生成表格文件写入服务端工作目录，按扩展名决定格式：.xlsx 为 Excel 工作簿，.csv 为逗号分隔文本。参数：fileName（相对路径，.xlsx 或 .csv）, sheetsJson（工作表数组 JSON：[{\"name\":\"汇总\",\"rows\":[[\"名称\",\"数量\"],[\"苹果\",3]]}]，单元格可给字符串/数字/布尔/null）, repo（可选，留空则写入工作目录 outputs 下）。第一个工作表的首行建议写表头。")]
    public async Task<string> SheetWriteAsync(
        [Description("表格相对路径，必须以 .xlsx 或 .csv 结尾")] string fileName,
        [Description("工作表数组的 JSON 字符串")] string sheetsJson,
        [Description("已克隆的仓库目录名或地址；留空则写入工作目录 outputs 下")] string? repo = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = (fileName ?? "").Trim();
        var isXlsx = trimmed.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        var isCsv = trimmed.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        if (!isXlsx && !isCsv)
        {
            return ToolEnvelope.Fail("fileName 必须以 .xlsx 或 .csv 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, trimmed, out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        List<SheetSpec> sheets;
        try
        {
            sheets = JsonSerializer.Deserialize<List<SheetSpec>>(sheetsJson, ReadJson) ?? [];
        }
        catch (JsonException ex)
        {
            return ToolEnvelope.Fail($"sheetsJson 不是合法 JSON 数组: {ex.Message}");
        }

        if (sheets.Count == 0)
        {
            return ToolEnvelope.Fail("sheetsJson 至少要包含一个工作表");
        }

        if (sheets.Count > MaxSheets)
        {
            return ToolEnvelope.Fail($"工作表数量超过上限 {MaxSheets}");
        }

        if (sheets.Any(s => s.Rows is { Count: > MaxRowsPerSheet }))
        {
            return ToolEnvelope.Fail($"单表行数超过上限 {MaxRowsPerSheet}");
        }

        if (isCsv && sheets.Count > 1)
        {
            return ToolEnvelope.Fail("csv 只能承载单个工作表，请改用 .xlsx 或只给一个表");
        }

        var dir = Path.GetDirectoryName(fullPath!)!;
        Directory.CreateDirectory(dir);

        var sheetNames = new List<string>();
        var rowCount = 0;

        using (await _locks.AcquireAsync(dir, cancellationToken))
        {
            if (isXlsx)
            {
                var writeError = WriteXlsx(fullPath!, sheets, out var writtenNames);
                if (writeError is not null)
                {
                    try { File.Delete(fullPath!); } catch (IOException) { /* 已由 error 说明 */ }
                    return ToolEnvelope.Fail(writeError);
                }

                sheetNames = writtenNames;
            }
            else
            {
                // Excel 在中文 Windows 上按本地编码猜 csv，没有 BOM 的 UTF-8 会乱码
                File.WriteAllText(fullPath!, BuildCsv(sheets[0].Rows ?? []), new UTF8Encoding(true));
                sheetNames.Add(sheets[0].Name ?? "Sheet1");
            }

            rowCount = sheets.Sum(s => s.Rows?.Count ?? 0);
        }

        var relative = Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/');
        return ToolEnvelope.Ok(new
        {
            file = relative,
            format = isXlsx ? "xlsx" : "csv",
            sizeBytes = new FileInfo(fullPath!).Length,
            sheets = sheetNames,
            rows = rowCount,
            note = "可用 SheetReadAsync 回读核对内容"
        });
    }

    [Description("回读表格内容（xlsx 或 csv），用于核对刚生成的表或读取用户提供的表。参数：fileName（相对路径）, sheet（工作表名，留空=第一个表；xlsx 会返回可选表名）, maxRows（默认 60）, repo（可选）。")]
    public async Task<string> SheetReadAsync(
        [Description("表格相对路径，必须以 .xlsx 或 .csv 结尾")] string fileName,
        [Description("工作表名称，留空则读第一个表")] string? sheet = null,
        [Description("最多回读行数")] int maxRows = 60,
        [Description("已克隆的仓库目录名或地址；留空则在工作目录 outputs 下查找")] string? repo = null,
        CancellationToken cancellationToken = default)
    {
        var trimmed = (fileName ?? "").Trim();
        var isXlsx = trimmed.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        var isCsv = trimmed.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        if (!isXlsx && !isCsv)
        {
            return ToolEnvelope.Fail("fileName 必须以 .xlsx 或 .csv 结尾");
        }

        if (!_resolver.TryResolveOutputPath(repo, trimmed, out var fullPath, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        if (!File.Exists(fullPath!))
        {
            return ToolEnvelope.Fail($"文件不存在: {fileName}");
        }

        var wantedRows = Math.Clamp(maxRows > 0 ? maxRows : 60, 1, 500);
        List<string> sheetNames = [];
        List<List<string?>> rows;
        int totalRows;

        using (await _locks.AcquireAsync(Path.GetDirectoryName(fullPath!)!, cancellationToken))
        {
            if (isXlsx)
            {
                using var doc = SpreadsheetDocument.Open(fullPath!, false);
                var workbookPart = doc.WorkbookPart;
                var workbook = workbookPart?.Workbook;
                if (workbook is null) return ToolEnvelope.Fail("工作簿缺少 workbook 部件");

                var shared = workbookPart!.SharedStringTablePart?.SharedStringTable;
                sheetNames = workbook.Descendants<Sheet>()
                    .Select(s => s.Name?.Value ?? "")
                    .ToList();

                var target = sheetNames.Count == 0
                    ? null
                    : sheetNames.FirstOrDefault(n => string.Equals(n, sheet, StringComparison.OrdinalIgnoreCase))
                      ?? sheetNames[0];

                var sheetElement = workbook.Descendants<Sheet>().First(s => (s.Name?.Value ?? "") == target);
                rows = [];
                totalRows = 0;

                var worksheetPart = workbookPart.GetPartById(sheetElement.Id!.Value) as WorksheetPart;
                if (worksheetPart?.Worksheet is null) return ToolEnvelope.Fail($"找不到工作表 {target} 的数据部件");
                foreach (var row in worksheetPart.Worksheet.Descendants<Row>())
                {
                    totalRows++;
                    if (rows.Count >= wantedRows) continue;

                    var cells = new List<string?>();
                    var column = 0;
                    foreach (var cell in row.Descendants<Cell>())
                    {
                        var index = ColumnIndexFromReference(cell.CellReference?.Value);
                        while (column < index)
                        {
                            cells.Add(null);
                            column++;
                        }

                        cells.Add(CellText(cell, shared));
                        column = index + 1;
                    }

                    rows.Add(cells);
                }
            }
            else
            {
                var parsed = ParseCsv(File.ReadAllText(fullPath!));
                totalRows = parsed.Count;
                rows = parsed.Take(wantedRows).Select(r => r.Select(v => (string?)v).ToList()).ToList();
                sheetNames.Add("csv");
            }
        }

        // 序列化后仍可能超出轨迹列宽，按行裁到预算内并如实标记 truncated
        var truncated = rows.Count < totalRows;
        var payload = BuildReadPayload(Path.GetRelativePath(_resolver.WorkDir, fullPath!).Replace('\\', '/'),
            isXlsx, sheetNames, rows, totalRows, ref truncated);
        return ToolEnvelope.Ok(payload);
    }

    private object BuildReadPayload(string fileName, bool isXlsx, List<string> sheetNames,
        List<List<string?>> rows, int totalRows, ref bool truncated)
    {
        var kept = rows;
        while (kept.Count > 1 && SerializedLength(kept) > ReadPayloadBudget)
        {
            kept = kept.Take(kept.Count - 1).ToList();
            truncated = true;
        }

        return new
        {
            file = fileName,
            format = isXlsx ? "xlsx" : "csv",
            sheets = sheetNames,
            totalRows,
            returnedRows = kept.Count,
            rows = kept,
            truncated
        };
    }

    private static int SerializedLength(List<List<string?>> rows) =>
        JsonSerializer.Serialize(rows).Length;

    private string? WriteXlsx(string path, List<SheetSpec> sheets, out List<string> names)
    {
        names = [];
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using (var doc = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
        {
            var workbookPart = doc.AddWorkbookPart();
            workbookPart.Workbook = new SpreadsheetWorkbook();
            var sheetsElement = workbookPart.Workbook.AppendChild(new Sheets());

            uint sheetId = 1;
            foreach (var spec in sheets)
            {
                var name = SanitizeSheetName(spec.Name, sheetId);
                if (!usedNames.Add(name))
                {
                    return $"工作表名重复: {spec.Name}";
                }

                names.Add(name);
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();

                var rows = spec.Rows ?? [];
                var columns = rows.Count == 0 ? 0 : rows.Max(r => r?.Count ?? 0);
                if (columns > MaxColumns)
                {
                    return $"列数超过上限 {MaxColumns}";
                }

                uint rowIndex = 1;
                foreach (var raw in rows)
                {
                    var row = new Row { RowIndex = rowIndex };
                    var count = raw?.Count ?? 0;
                    var nonEmpty = false;

                    for (var c = 0; c < count; c++)
                    {
                        var cell = BuildCell($"{GetColumnName(c)}{rowIndex}", raw![c]);
                        if (cell is not null)
                        {
                            row.Append(cell);
                            nonEmpty = true;
                        }
                    }

                    if (nonEmpty) sheetData.Append(row);
                    rowIndex++;
                }

                if (columns > 0)
                {
                    var cols = new Columns();
                    for (ushort c = 1; c <= columns; c++)
                    {
                        cols.Append(new Column
                        {
                            Min = c,
                            Max = c,
                            Width = 22d
                        });
                    }

                    worksheetPart.Worksheet = new Worksheet(cols, sheetData);
                }
                else
                {
                    worksheetPart.Worksheet = new Worksheet(sheetData);
                }

                sheetsElement.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = sheetId,
                    Name = name
                });
                sheetId++;
            }

            workbookPart.Workbook.Save();

            var errors = new OpenXmlValidator().Validate(doc).ToList();
            if (errors.Count > 0)
            {
                _logger.LogWarning("生成的 xlsx 未通过 OpenXml 校验: {File}, {Count} 个错误: {First}",
                    path, errors.Count, errors[0].Description);
                return $"生成的表格结构校验未通过（{errors.Count} 个问题）：{errors[0].Description}";
            }
        }

        return null;
    }

    private static Cell? BuildCell(string reference, JsonElement? value)
    {
        if (value is null) return null;

        var element = value.Value;
        switch (element.ValueKind)
        {
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            case JsonValueKind.Number:
                return new Cell
                {
                    CellReference = reference,
                    DataType = CellValues.Number,
                    CellValue = new CellValue(element.GetRawText())
                };
            case JsonValueKind.True:
            case JsonValueKind.False:
                return new Cell
                {
                    CellReference = reference,
                    DataType = CellValues.Boolean,
                    CellValue = new CellValue(element.ValueKind == JsonValueKind.True ? "1" : "0")
                };
            default:
            {
                var text = element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText();
                if (string.IsNullOrEmpty(text)) return null;

                // 能被识别为数字的字符串仍按数字写入，否则 Excel 里全是"以文本形式存储的数字"
                if (double.TryParse(text, NumberStyles.Float | NumberStyles.AllowThousands,
                        CultureInfo.InvariantCulture, out var numeric) && text!.Length <= 15)
                {
                    return new Cell
                    {
                        CellReference = reference,
                        DataType = CellValues.Number,
                        CellValue = new CellValue(numeric.ToString(CultureInfo.InvariantCulture))
                    };
                }

                return new Cell
                {
                    CellReference = reference,
                    DataType = CellValues.InlineString,
                    InlineString = new InlineString(new DocumentFormat.OpenXml.Spreadsheet.Text(text)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    })
                };
            }
        }
    }

    private static string? CellText(Cell cell, SharedStringTable? shared)
    {
        var type = cell.DataType?.Value;
        if (type == CellValues.SharedString)
        {
            var indexText = cell.CellValue?.InnerText;
            if (int.TryParse(indexText, out var index) && shared is not null)
            {
                return shared.Elements<SharedStringItem>().ElementAtOrDefault(index)?.InnerText ?? indexText;
            }

            return indexText;
        }

        if (type == CellValues.InlineString)
        {
            return cell.InlineString?.InnerText;
        }

        var raw = cell.CellValue?.InnerText;
        if (type == CellValues.Boolean)
        {
            return raw == "1" ? "TRUE" : raw == "0" ? "FALSE" : raw;
        }

        return string.IsNullOrEmpty(raw) ? cell.InnerText : raw;
    }

    private static string SanitizeSheetName(string? name, uint fallbackIndex)
    {
        var value = (name ?? "").Trim();
        if (value.Length == 0) value = $"Sheet{fallbackIndex}";

        var sb = new StringBuilder();
        foreach (var c in value)
        {
            sb.Append(c is ':' or '\\' or '/' or '?' or '*' or '[' or ']' ? '_' : c);
        }

        var cleaned = sb.ToString();
        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }

    private static string GetColumnName(int index)
    {
        var name = "";
        var n = index + 1;
        while (n > 0)
        {
            var remainder = (n - 1) % 26;
            name = (char)('A' + remainder) + name;
            n = (n - 1) / 26;
        }

        return name;
    }

    private static int ColumnIndexFromReference(string? reference)
    {
        if (string.IsNullOrEmpty(reference)) return 0;

        var index = 0;
        foreach (var c in reference)
        {
            if (c >= 'A' && c <= 'Z') index = index * 26 + (c - 'A' + 1);
            else if (c >= 'a' && c <= 'z') index = index * 26 + (c - 'a' + 1);
            else break;
        }

        return Math.Max(0, index - 1);
    }

    private static string BuildCsv(List<List<JsonElement?>?> rows)
    {
        var sb = new StringBuilder();
        foreach (var row in rows)
        {
            var cells = (row ?? []).Select(CellToText);
            sb.AppendLine(string.Join(",", cells.Select(EscapeCsv)));
        }

        return sb.ToString().Replace("\r\n", "\n").Replace("\n", "\r\n");
    }

    private static string CellToText(JsonElement? value)
    {
        if (value is null) return "";
        var element = value.Value;
        return element.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => "",
            JsonValueKind.String => element.GetString() ?? "",
            JsonValueKind.True => "TRUE",
            JsonValueKind.False => "FALSE",
            _ => element.GetRawText()
        };
    }

    private static string EscapeCsv(string value)
    {
        if (value.Length == 0) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    /// <summary>
    /// 最小 RFC4180 解析：双引号包裹、"" 转义、允许字段内含换行。
    /// 不用 Split(',')：带逗号的字段会被劈成两列，读回来的表就成了假数据。
    /// </summary>
    private static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var current = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var i = 0;

        void EndField()
        {
            current.Add(field.ToString());
            field.Clear();
        }

        void EndRow()
        {
            EndField();
            rows.Add(current);
            current = new List<string>();
        }

        while (i < text.Length)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                        continue;
                    }

                    inQuotes = false;
                    i++;
                    continue;
                }

                field.Append(c);
                i++;
                continue;
            }

            switch (c)
            {
                case '"':
                    inQuotes = true;
                    i++;
                    break;
                case ',':
                    EndField();
                    i++;
                    break;
                case '\r':
                    if (i + 1 < text.Length && text[i + 1] == '\n') i++;
                    i++;
                    EndRow();
                    break;
                case '\n':
                    i++;
                    EndRow();
                    break;
                default:
                    field.Append(c);
                    i++;
                    break;
            }
        }

        if (field.Length > 0 || current.Count > 0) EndRow();

        // 首尾空行是记事本/Excel 的常态，不该被当成数据
        return rows.Where(r => r.Any(v => !string.IsNullOrWhiteSpace(v))).ToList();
    }

    private sealed class SheetSpec
    {
        public string? Name { get; set; }
        public List<List<JsonElement?>?>? Rows { get; set; }
    }
}
