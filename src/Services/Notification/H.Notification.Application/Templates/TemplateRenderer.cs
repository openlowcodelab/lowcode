using System.Text.RegularExpressions;

namespace H.Notification.Application.Templates;

/// <summary>
/// 模板渲染工具：将 {{key}} 占位符替换为变量值
/// </summary>
public static class TemplateRenderer
{
    private static readonly Regex PlaceholderRegex = new(@"\{\{\s*(\w+)\s*\}\}", RegexOptions.Compiled);

    public static string? Render(string? template, IReadOnlyDictionary<string, string> data)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return data.TryGetValue(key, out var value) ? value ?? string.Empty : match.Value;
        });
    }
}
