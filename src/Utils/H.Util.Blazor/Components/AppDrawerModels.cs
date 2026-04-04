namespace H.Util.Blazor;

/// <summary>
/// 应用分类信息
/// </summary>
public class AppCategoryInfo
{
    /// <summary>
    /// 分类名称
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 分类图标（可选）
    /// </summary>
    public string? CategoryIcon { get; set; }

    /// <summary>
    /// 应用列表
    /// </summary>
    public List<AppItemInfo> Apps { get; set; } = new();
}

/// <summary>
/// 应用项信息
/// </summary>
public class AppItemInfo
{
    /// <summary>
    /// 应用唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 应用名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 应用图标（Emoji 或 Unicode 符号）
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 应用图标 URL（如果使用图片图标）
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// 应用跳转地址
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 打开方式：_self（当前页）或 _blank（新标签页）
    /// </summary>
    public string Target { get; set; } = "_self";

    /// <summary>
    /// 应用描述（可选）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否启用（可选，用于控制应用是否显示）
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 排序号（可选）
    /// </summary>
    public int Order { get; set; } = 0;
}
