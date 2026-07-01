namespace H.AppDrawer.Components;

/// <summary>
/// 应用菜单项
/// </summary>
public class AppMenuItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";

    /// <summary>
    /// 菜单唯一标识（用于展开/折叠状态跟踪）
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 子菜单项
    /// </summary>
    public List<AppMenuItem>? Children { get; set; }
}
