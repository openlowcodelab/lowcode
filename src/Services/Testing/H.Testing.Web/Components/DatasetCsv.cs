namespace H.Testing.Web;

/// <summary>
/// 数据集 CSV 文本解析与序列化（首行为列名）
/// </summary>
internal static class DatasetCsv
{
    /// <summary>
    /// 解析 CSV 文本；首行为列名且至少有一行数据时返回 true
    /// </summary>
    public static bool TryParse(
        string csvText,
        out List<string> columns,
        out List<Dictionary<string, string>> rows)
    {
        columns = [];
        rows = [];

        var lines = csvText.Replace("\r", "").Split('\n')
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        if (lines.Count < 2)
        {
            return false;
        }

        columns = ParseCsvLine(lines[0]);
        if (columns.Count == 0)
        {
            columns = [];
            return false;
        }

        foreach (var line in lines.Skip(1))
        {
            var values = ParseCsvLine(line);
            var row = new Dictionary<string, string>();
            for (var i = 0; i < columns.Count; i++)
            {
                row[columns[i]] = i < values.Count ? values[i] : string.Empty;
            }
            rows.Add(row);
        }

        return true;
    }

    /// <summary>
    /// 简易 CSV 行解析（支持双引号包裹与引号转义）
    /// </summary>
    public static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(ch);
                }
            }
            else if (ch == '"')
            {
                inQuotes = true;
            }
            else if (ch == ',')
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    public static string ToCsv(List<string> columns, List<Dictionary<string, string>> rows)
    {
        var lines = new List<string> { string.Join(",", columns.Select(EscapeCsv)) };
        foreach (var row in rows)
        {
            lines.Add(string.Join(",", columns.Select(c => EscapeCsv(row.GetValueOrDefault(c, "")))));
        }
        return string.Join("\n", lines);
    }

    public static string EscapeCsv(string value)
        => value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
