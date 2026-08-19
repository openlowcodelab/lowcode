using System.Text;
using System.Text.Json;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// 表达式求值上下文
/// </summary>
public class LowCodeExpressionContext
{
    /// <summary>
    /// 当前行数据（列表渲染上下文）
    /// </summary>
    public object? Item { get; set; }

    /// <summary>
    /// 页面表单状态
    /// </summary>
    public PageFormStateService? FormState { get; set; }

    /// <summary>
    /// URL 参数提供器
    /// </summary>
    public Func<string, string?>? QueryProvider { get; set; }
}

/// <summary>
/// 低代码表达式解析器 - 平台层通用能力
/// </summary>
/// <remarks>
/// 支持的表达式：
/// $query(name)            URL 参数
/// $(item.field)           当前行数据字段
/// $(form.key)             表单状态值
/// $(form[innerExpr])      表单状态值（key 为表达式，递归求值）
/// $(formjson(listId,compName)) 聚合列表实例表单值为 JSON 数组
/// $(now)                  当前时间（yyyy-MM-dd HH:mm:ss）
/// 表达式可与普通文本混合，混合时结果为字符串；整个表达式串为单一表达式时返回原始类型值。
/// </remarks>
public static class LowCodeExpressionResolver
{
    /// <summary>
    /// 判断字符串是否包含表达式
    /// </summary>
    public static bool ContainsExpression(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.Contains("$(") || text.Contains("$query(");
    }

    /// <summary>
    /// 求值：整个字符串为单一表达式时返回原始类型值，否则返回插值后的字符串
    /// </summary>
    public static object? Resolve(string? expression, LowCodeExpressionContext context)
    {
        if (string.IsNullOrEmpty(expression))
            return expression;

        // 整个字符串恰为单一表达式时，返回原始类型值
        if (TryGetSingleToken(expression, out var tokenStart, out var tokenEnd))
        {
            if (tokenStart == 0 && tokenEnd == expression.Length)
                return ResolveToken(expression, 0, expression.Length, context);
        }

        var sb = new StringBuilder();
        int pos = 0;
        while (pos < expression.Length)
        {
            if (TryGetSingleToken(expression.Substring(pos), out var relStart, out var relEnd))
            {
                sb.Append(expression, pos, relStart);
                var value = ResolveToken(expression, pos + relStart, pos + relEnd, context);
                sb.Append(FormatValue(value));
                pos += relEnd;
            }
            else
            {
                sb.Append(expression, pos, expression.Length - pos);
                break;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 求值为字符串
    /// </summary>
    public static string? ResolveAsString(string? expression, LowCodeExpressionContext context)
    {
        var value = Resolve(expression, context);
        return value == null ? null : FormatValue(value);
    }

    /// <summary>
    /// 将值格式化为字符串（布尔为 true/false，时间为 yyyy-MM-dd HH:mm:ss）
    /// </summary>
    public static string FormatValue(object? value)
    {
        if (value == null)
            return string.Empty;

        if (value is bool boolValue)
            return boolValue ? "true" : "false";

        if (value is DateTime dateTime)
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 在字符串中查找第一个表达式标记，返回相对起止位置（不含 $ 前缀起点到闭括号后一位）
    /// </summary>
    private static bool TryGetSingleToken(string text, out int start, out int end)
    {
        start = 0;
        end = 0;

        int dollarIndex = text.IndexOf('$');
        while (dollarIndex >= 0)
        {
            // $query(
            if (dollarIndex + "$query(".Length <= text.Length
                && text.Substring(dollarIndex, "$query(".Length) == "$query(")
            {
                var close = FindMatchingParen(text, dollarIndex + "$query(".Length - 1);
                if (close > 0)
                {
                    start = dollarIndex;
                    end = close + 1;
                    return true;
                }
            }
            // $(
            else if (dollarIndex + 1 < text.Length && text[dollarIndex + 1] == '(')
            {
                var close = FindMatchingParen(text, dollarIndex + 1);
                if (close > 0)
                {
                    start = dollarIndex;
                    end = close + 1;
                    return true;
                }
            }

            dollarIndex = text.IndexOf('$', dollarIndex + 1);
        }

        return false;
    }

    /// <summary>
    /// 从开括号位置查找匹配的闭括号
    /// </summary>
    private static int FindMatchingParen(string text, int openIndex)
    {
        int depth = 0;
        for (int i = openIndex; i < text.Length; i++)
        {
            if (text[i] == '(')
                depth++;
            else if (text[i] == ')')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 求值单个表达式标记（text[start..end] 形如 $(...) 或 $query(...)）
    /// </summary>
    private static object? ResolveToken(string text, int start, int end, LowCodeExpressionContext context)
    {
        var token = text.Substring(start, end - start);

        // $query(name)，支持 $query(name1|name2) 依次回退取第一个非空参数
        if (token.StartsWith("$query("))
        {
            var name = token.Substring("$query(".Length, token.Length - "$query(".Length - 1).Trim();
            if (context.QueryProvider == null)
                return null;

            foreach (var candidate in name.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                var queryValue = context.QueryProvider(candidate);
                if (!string.IsNullOrEmpty(queryValue))
                    return queryValue;
            }

            return null;
        }

        // $(...)
        var content = token.Substring(2, token.Length - 3).Trim();

        // $(now)
        if (content == "now")
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // $(item.field)
        if (content.StartsWith("item."))
        {
            var fieldName = content.Substring("item.".Length);
            return GetMemberValue(context.Item, fieldName);
        }

        // $(form[innerExpr])
        if (content.StartsWith("form[") && content.EndsWith("]"))
        {
            var innerExpr = content.Substring("form[".Length,
                content.Length - "form[".Length - 1);
            var key = ResolveAsString(innerExpr, context);
            return string.IsNullOrEmpty(key) ? null : context.FormState?.GetValue(key);
        }

        // $(formjson(listId,compName))
        if (content.StartsWith("formjson(") && content.EndsWith(")"))
        {
            var args = content.Substring("formjson(".Length,
                content.Length - "formjson(".Length - 1)
                .Split(',', StringSplitOptions.TrimEntries);
            if (args.Length != 2 || context.FormState == null)
                return null;

            var values = context.FormState.GetListInstanceValues(args[0], args[1]);
            var items = values.Select(kv => new Dictionary<string, object?>
            {
                ["id"] = kv.Key,
                ["value"] = kv.Value
            });
            return JsonSerializer.Serialize(items);
        }

        // $(form.key)
        if (content.StartsWith("form."))
        {
            var key = content.Substring("form.".Length);
            return context.FormState?.GetValue(key);
        }

        // 未识别的表达式保留原文（兼容如 $(DraggableContainer) 等占位标记）
        return token;
    }

    /// <summary>
    /// 从行数据对象中获取字段值（支持字典与反射属性）
    /// </summary>
    public static object? GetMemberValue(object? dataItem, string fieldName)
    {
        if (dataItem == null || string.IsNullOrEmpty(fieldName))
            return null;

        if (dataItem is Dictionary<string, object> dict)
            return dict.TryGetValue(fieldName, out var value) ? value : null;

        if (dataItem is IDictionary<string, object> genericDict)
            return genericDict.TryGetValue(fieldName, out var value2) ? value2 : null;

        var propertyInfo = dataItem.GetType().GetProperty(fieldName);
        return propertyInfo?.GetValue(dataItem);
    }
}
