using H.Admin.AppDrawer;
using System;

namespace H.Admin.AppDrawer;

/// <summary>
/// 应用状态管理器，负责管理当前应用状态和应用列表
/// </summary>
public class AppStateManager
{
    private string _currentAppId = "portal";
    private AppCategoryInfo[]? _categories;
    private readonly string _jsonFilePath;

    public AppStateManager()
    {
        _jsonFilePath = FindAppsJsonFile();
    }

    /// <summary>
    /// 查找 apps.json 文件
    /// </summary>
    private string FindAppsJsonFile()
    {
#if DEBUG
        var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "Services", "Portal", "data", "apps.json");
        return jsonFilePath;
#else
        var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "apps.json");
        return jsonFilePath;
#endif
    }

    /// <summary>
    /// 当前应用ID
    /// </summary>
    public string CurrentAppId
    {
        get => _currentAppId;
        set
        {
            _currentAppId = value;
            OnAppChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// 应用切换事件
    /// </summary>
    public event Action<string>? OnAppChanged;

    /// <summary>
    /// 获取应用分类列表
    /// </summary>
    public AppCategoryInfo[] GetAppCategories()
    {
        if (_categories == null)
        {
            LoadAppsFromJson();
        }
        return _categories ?? [];
    }

    /// <summary>
    /// 从 JSON 文件加载应用数据
    /// </summary>
    private void LoadAppsFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                // 如果文件不存在，使用默认数据
                _categories = [];
                return;
            }

            var jsonContent = File.ReadAllText(_jsonFilePath);
            var appData = jsonContent.FromJson<AppData>();
            
            _categories = appData?.AppCategories?.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            // 如果加载失败，使用默认数据
            Console.WriteLine($"Failed to load apps from JSON: {ex.Message}");
            _categories = [];
        }
    }

    /// <summary>
    /// 获取当前应用的菜单标识
    /// </summary>
    public string GetCurrentAppMenuKey()
    {
        return _currentAppId switch
        {
            "account" => "account-menu",
            "organization" => "org-menu",
            "design-engine" => "design-menu",
            "render-engine" => "render-menu",
            "approval" => "approval-menu",
            "autotest" => "autotest-menu",
            _ => "portal-menu"
        };
    }

    /// <summary>
    /// 获取指定应用的菜单项列表
    /// </summary>
    public List<AppMenuItem> GetAppMenuItems(string appId)
    {
        if (_categories == null)
        {
            LoadAppsFromJson();
        }

        // 从加载的应用数据中查找菜单项
        foreach (var category in _categories ?? [])
        {
            var app = category.Apps?.FirstOrDefault(a => a.Id == appId);
            if (app != null && app.MenuItems != null)
            {
                return app.MenuItems;
            }
        }

        // 如果未找到，返回空列表
        return new List<AppMenuItem>();
    }

    /// <summary>
    /// 重新加载应用数据（用于管理页面更新后刷新）
    /// </summary>
    public void ReloadApps()
    {
        _categories = null;
        LoadAppsFromJson();
    }
}
