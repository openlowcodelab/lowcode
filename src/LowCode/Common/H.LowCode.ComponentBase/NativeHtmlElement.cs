using H.LowCode.MetaSchema;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 原生 html 元素物料支持。
/// 物料 JSON 的 frag.dt / frag.t 使用 "html:{tag}" 形式（如 "html:button"、"html:input"）直接声明原生 html 标签，
/// 渲染引擎据此用 OpenElement 渲染原生元素，而无需编译好的 Razor 组件。
/// </summary>
public static class NativeHtmlElement
{
    public const string Prefix = "html:";

    /// <summary>
    /// 选项数据源渲染时的值占位符（替换为选项 Value）
    /// </summary>
    public const string OptionValueToken = "$(value)";

    /// <summary>
    /// 选项数据源渲染时的文本占位符（替换为选项 Label）
    /// </summary>
    public const string OptionLabelToken = "$(label)";

    /// <summary>
    /// 容器内容占位标记（设计时注入 DraggableContainer）
    /// </summary>
    public const string DraggableContainerToken = "$(DraggableContainer)";

    public static bool IsNativeHtml(string? typeName)
        => typeName?.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) == true;

    public static string? GetTagName(string? typeName)
    {
        if (!IsNativeHtml(typeName))
            return null;

        return typeName![Prefix.Length..].Trim().ToLowerInvariant();
    }

    /// <summary>
    /// 将组件事件名（如 OnClick / OnChange）映射为元素事件名（onclick / onchange）
    /// </summary>
    public static string? ToElementEventName(string? eventName)
    {
        if (string.IsNullOrEmpty(eventName))
            return null;

        return eventName.ToLowerInvariant();
    }

    /// <summary>
    /// 判断是否为表单控件元素（需要 Value 双向绑定）
    /// </summary>
    public static bool IsFormControl(string tagName, string? typeAttribute)
    {
        if (tagName is "textarea" or "select")
            return true;

        if (tagName == "input")
        {
            // 常见可输入类型；hidden/button/submit/image/reset/checkbox/radio 中仅 checkbox/radio 需 checked 绑定，另行处理
            return typeAttribute is null or "text" or "number" or "date" or "time" or "month" or "week"
                or "password" or "email" or "search" or "tel" or "url" or "datetime-local";
        }

        return false;
    }

    /// <summary>
    /// 解析 attrn 的路径前缀约定。
    /// 根 Fragment 的属性 attrn 为普通 html 属性名（如 "placeholder"、"class"）或 "content"（文本内容）；
    /// 嵌套子元素的属性用 "childs.{i}..." 路径（如 "childs.0.content"、"childs.1.childs.0.class"）。
    /// 返回 (定位路径, 属性名)；路径为空串表示根。定位路径形如 "childs.1.childs.0"。
    /// </summary>
    public static (string Path, string AttributeName) ParseAttributePath(string? attributeName)
    {
        if (string.IsNullOrEmpty(attributeName))
            return (string.Empty, attributeName ?? string.Empty);

        var lastDot = attributeName.LastIndexOf('.');
        if (lastDot < 0)
            return (string.Empty, attributeName);

        var path = attributeName[..lastDot];
        if (!path.StartsWith("childs.", StringComparison.OrdinalIgnoreCase))
            return (string.Empty, attributeName);

        return (path.ToLowerInvariant(), attributeName[(lastDot + 1)..]);
    }

    /// <summary>
    /// 计算子元素的路径（用于路径属性定位）
    /// </summary>
    public static string ChildPath(string parentPath, int childIndex)
        => string.IsNullOrEmpty(parentPath) ? $"childs.{childIndex}" : $"{parentPath}.childs.{childIndex}";

    /// <summary>
    /// 选项数据源渲染：将属性值/文本中的 $(value)/$(label) 占位符替换为实际选项值
    /// </summary>
    public static string? SubstituteOptionToken(string? value, string? optionValue, string? optionLabel)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = value.Replace(OptionValueToken, optionValue ?? string.Empty)
            .Replace(OptionLabelToken, optionLabel ?? string.Empty);
        return result;
    }
}
