using Microsoft.AspNetCore.Components;

namespace H.AppDrawer.Components;

/// <summary>
/// 统一菜单图标 (内联 SVG, 描边使用 currentColor, 尺寸随字号 1em)
/// 顶部导航与侧边菜单共用, 保证图标风格一致, 替代原 emoji 图标
/// </summary>
public static class MenuIcons
{
    private const string SvgOpen =
        "<svg viewBox=\"0 0 24 24\" width=\"1em\" height=\"1em\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" style=\"vertical-align:-0.14em\">";
    private const string SvgClose = "</svg>";

    public static MarkupString GetSvg(string? icon)
    {
        var path = icon switch
        {
            "home" => "<path d=\"M3 9.5 12 3l9 6.5\"/><path d=\"M5 10v10h14V10\"/>",
            "dashboard" => "<rect x=\"3\" y=\"3\" width=\"8\" height=\"10\" rx=\"1\"/><rect x=\"13\" y=\"3\" width=\"8\" height=\"6\" rx=\"1\"/><rect x=\"13\" y=\"11\" width=\"8\" height=\"10\" rx=\"1\"/><rect x=\"3\" y=\"15\" width=\"8\" height=\"6\" rx=\"1\"/>",
            "appstore" => "<rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"7\" rx=\"1\"/><rect x=\"3\" y=\"14\" width=\"7\" height=\"7\" rx=\"1\"/><rect x=\"14\" y=\"14\" width=\"7\" height=\"7\" rx=\"1\"/>",
            "api" => "<path d=\"M4 12h4\"/><path d=\"M16 12h4\"/><rect x=\"8\" y=\"8\" width=\"8\" height=\"8\" rx=\"2\"/><path d=\"M12 4v4\"/><path d=\"M12 16v4\"/>",
            "cloud" or "cloud-upload" => "<path d=\"M17 18a4 4 0 0 0 0-8 6 6 0 0 0-11.5-1.5A3.5 3.5 0 0 0 6 18z\"/>",
            "deployment-unit" => "<circle cx=\"12\" cy=\"5\" r=\"2\"/><circle cx=\"5\" cy=\"18\" r=\"2\"/><circle cx=\"19\" cy=\"18\" r=\"2\"/><path d=\"M12 7v4M12 11 6 16M12 11l6 5\"/>",
            "question-circle" => "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M9.5 9a2.5 2.5 0 1 1 3.5 2.3c-.8.4-1 .9-1 1.7\"/><circle cx=\"12\" cy=\"16.5\" r=\"0.6\" fill=\"currentColor\"/>",
            "profile" or "contacts" => "<rect x=\"4\" y=\"3\" width=\"16\" height=\"18\" rx=\"2\"/><path d=\"M8 7h8M8 11h8M8 15h5\"/>",
            "menu" or "bars" => "<path d=\"M4 6h16M4 12h16M4 18h16\"/>",
            "database" => "<ellipse cx=\"12\" cy=\"5\" rx=\"8\" ry=\"3\"/><path d=\"M4 5v6c0 1.7 3.6 3 8 3s8-1.3 8-3V5\"/><path d=\"M4 11v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6\"/>",
            "tool" => "<path d=\"M14.7 6.3a4 4 0 0 0-5.4 5.4L4 17l3 3 5.3-5.3a4 4 0 0 0 5.4-5.4l-2.3 2.3-2-2z\"/>",
            "setting" => "<circle cx=\"12\" cy=\"12\" r=\"3\"/><path d=\"M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z\"/>",
            "edit" => "<path d=\"M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7\"/><path d=\"M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z\"/>",
            "user" => "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 20c0-3.5 3.5-6 8-6s8 2.5 8 6\"/>",
            "team" => "<circle cx=\"9\" cy=\"8\" r=\"3\"/><path d=\"M4 20a5 5 0 0 1 10 0\"/><path d=\"M16 5.5a3 3 0 0 1 0 5.8\"/><path d=\"M17 15.5a5 5 0 0 1 3 4.5\"/>",
            "clock-circle" or "schedule" => "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 7v5l3.5 2\"/>",
            "swap" => "<path d=\"M7 4v13\"/><path d=\"M3.5 13.5 7 17l3.5-3.5\"/><path d=\"M17 20V7\"/><path d=\"M13.5 10.5 17 7l3.5 3.5\"/>",
            "shopping" => "<path d=\"M6 8h12l-1 12H7L6 8z\"/><path d=\"M9 8V6a3 3 0 0 1 6 0v2\"/>",
            "bar-chart" => "<path d=\"M5 20V10\"/><path d=\"M12 20V4\"/><path d=\"M19 20v-8\"/>",
            "global" => "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M3 12h18\"/><path d=\"M12 3a15 15 0 0 1 4 9 15 15 0 0 1-4 9 15 15 0 0 1-4-9 15 15 0 0 1 4-9z\"/>",
            "desktop" => "<rect x=\"3\" y=\"4\" width=\"18\" height=\"12\" rx=\"2\"/><path d=\"M8 20h8M12 16v4\"/>",
            "rocket" => "<path d=\"M12 3c3 1 6 4 6 9l-3 3H9l-3-3c0-5 3-8 6-9z\"/><circle cx=\"12\" cy=\"9\" r=\"1.5\"/><path d=\"M9 18c-1 1-1 3-1 3s2 0 3-1M15 18c1 1 1 3 1 3s-2 0-3-1\"/>",
            _ => null
        };

        if (path == null)
        {
            // 无匹配: emoji/非 ASCII 文本原样返回; 其余(未知图标名)给默认文档图标, 避免菜单中显示原始文本
            if (!string.IsNullOrEmpty(icon) && icon.Any(t => t > 127))
                return new MarkupString(icon);
            path = "<rect x=\"5\" y=\"3\" width=\"14\" height=\"18\" rx=\"2\"/><path d=\"M9 7h6M9 11h6M9 15h4\"/>";
        }

        return new MarkupString($"{SvgOpen}{path}{SvgClose}");
    }
}
