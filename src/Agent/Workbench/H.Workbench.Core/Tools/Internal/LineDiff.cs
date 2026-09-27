namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// 行级 unified diff（LCS 回溯）。只用于把"改了什么"变成可验收的证据，
/// 不追求 git diff 的全部特性。
/// </summary>
public static class LineDiff
{
    /// <summary>参与比对的最大行数：超过则只报统计，避免大文件把内存和上下文一起撑爆</summary>
    private const int MaxLines = 4000;

    private const int ContextLines = 3;

    public static string? Unified(string path, string before, string after, int maxChars = 6000)
    {
        var a = Split(before);
        var b = Split(after);
        if (a.Length > MaxLines || b.Length > MaxLines) return null;

        var ops = DiffOps(a, b);

        // 标出变更行，再按上下文切 hunk
        var changed = new bool[ops.Length];
        for (var i = 0; i < ops.Length; i++)
        {
            changed[i] = ops[i].Op != ' ';
        }

        var groups = new List<(int Start, int End)>();
        for (var i = 0; i < ops.Length; i++)
        {
            if (!changed[i]) continue;

            var start = Math.Max(0, i - ContextLines);
            if (groups.Count > 0 && start <= groups[^1].End)
            {
                var prev = groups[^1];
                groups[^1] = (prev.Start, Math.Min(ops.Length, i + ContextLines + 1));
            }
            else
            {
                groups.Add((start, Math.Min(ops.Length, i + ContextLines + 1)));
            }
        }

        if (groups.Count == 0) return null;

        var sb = new System.Text.StringBuilder();
        sb.Append("--- a/").Append(path).Append('\n');
        sb.Append("+++ b/").Append(path).Append('\n');

        foreach (var (start, end) in groups)
        {
            var aLine = 1;
            var bLine = 1;
            for (var i = 0; i < start; i++)
            {
                if (ops[i].Op != '+') aLine++;
                if (ops[i].Op != '-') bLine++;
            }

            var aCount = 0;
            var bCount = 0;
            for (var i = start; i < end; i++)
            {
                if (ops[i].Op != '+') aCount++;
                if (ops[i].Op != '-') bCount++;
            }

            sb.Append("@@ -").Append(aLine).Append(',').Append(aCount)
              .Append(" +").Append(bLine).Append(',').Append(bCount).Append(" @@\n");

            for (var i = start; i < end; i++)
            {
                sb.Append(ops[i].Op).Append(ops[i].Text).Append('\n');
            }
        }

        var text = sb.ToString();
        return text.Length > maxChars ? text[..maxChars] + "\n…[diff 已截断]" : text;
    }

    private static (char Op, string Text)[] DiffOps(string[] a, string[] b)
    {
        // 先剥掉首尾公共部分，缩小 LCS 矩阵
        var prefix = 0;
        while (prefix < a.Length && prefix < b.Length && a[prefix] == b[prefix]) prefix++;
        var suffix = 0;
        while (suffix < a.Length - prefix && suffix < b.Length - prefix
               && a[a.Length - 1 - suffix] == b[b.Length - 1 - suffix]) suffix++;

        var midA = a[prefix..(a.Length - suffix)];
        var midB = b[prefix..(b.Length - suffix)];

        var result = new List<(char, string)>();
        for (var i = 0; i < prefix; i++) result.Add((' ', a[i]));

        var lcs = LcsScript(midA, midB);
        result.AddRange(lcs);

        for (var i = a.Length - suffix; i < a.Length; i++) result.Add((' ', a[i]));
        return result.ToArray();
    }

    private static List<(char, string)> LcsScript(string[] a, string[] b)
    {
        if (a.Length == 0 && b.Length == 0) return [];
        if (a.Length == 0) return b.Select(x => ('+', x)).ToList();
        if (b.Length == 0) return a.Select(x => ('-', x)).ToList();

        var m = new int[a.Length + 1, b.Length + 1];
        for (var i = a.Length - 1; i >= 0; i--)
        {
            for (var j = b.Length - 1; j >= 0; j--)
            {
                m[i, j] = a[i] == b[j] ? m[i + 1, j + 1] + 1 : Math.Max(m[i + 1, j], m[i, j + 1]);
            }
        }

        var ops = new List<(char, string)>();
        int x = 0, y = 0;
        while (x < a.Length && y < b.Length)
        {
            if (a[x] == b[y])
            {
                ops.Add((' ', a[x]));
                x++; y++;
            }
            else if (m[x + 1, y] >= m[x, y + 1])
            {
                ops.Add(('-', a[x]));
                x++;
            }
            else
            {
                ops.Add(('+', b[y]));
                y++;
            }
        }
        while (x < a.Length) ops.Add(('-', a[x++]));
        while (y < b.Length) ops.Add(('+', b[y++]));
        return ops;
    }

    private static string[] Split(string text) =>
        string.IsNullOrEmpty(text) ? [] : text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
}
