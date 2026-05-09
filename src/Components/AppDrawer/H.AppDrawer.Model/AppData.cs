using System;

namespace H.Admin.AppDrawer;

/// <summary>
/// 应用数据根对象（用于 JSON 反序列化）
/// </summary>
public class AppData
{
    public List<AppCategoryInfo> AppCategories { get; set; } = [];
}
