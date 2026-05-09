using System;

namespace H.Admin.AppDrawer;

/// <summary>
/// 应用状态管理器，负责管理当前应用状态和应用列表
/// </summary>
public class AppStateManager
{
    private string _currentAppId = "portal";
    private List<AppCategoryInfo>? _categories;
    private readonly string _jsonFilePath;

    public AppStateManager(string? jsonFilePath = null)
    {
        _jsonFilePath = jsonFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Admin", "data", "apps.json");
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
    public List<AppCategoryInfo> GetAppCategories()
    {
        if (_categories == null)
        {
            LoadAppsFromJson();
        }
        return _categories ?? new List<AppCategoryInfo>();
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
                _categories = GetDefaultAppCategories();
                return;
            }

            var jsonContent = File.ReadAllText(_jsonFilePath);
            var appData = jsonContent.FromJson<AppData>();
            
            _categories = appData?.Categories ?? new List<AppCategoryInfo>();
        }
        catch (Exception ex)
        {
            // 如果加载失败，使用默认数据
            Console.WriteLine($"Failed to load apps from JSON: {ex.Message}");
            _categories = GetDefaultAppCategories();
        }
    }

    /// <summary>
    /// 获取默认应用分类（硬编码的备用数据）
    /// </summary>
    private List<AppCategoryInfo> GetDefaultAppCategories()
    {
        return new List<AppCategoryInfo>
        {
            new() {
                CategoryName = "基础服务",
                Apps =
                [
                    new() {
                        Id = "account",
                        Name = "用户管理",
                        Icon = "👤",
                        Url = "/account/users",
                        Target = "_self"
                    },
                    new() {
                        Id = "organization",
                        Name = "组织管理",
                        Icon = "🏢",
                        Url = "/organization",
                        Target = "_self"
                    },
                    new() {
                        Id = "approval",
                        Name = "审批管理",
                        Icon = "✅",
                        Url = "/approval",
                        Target = "_self"
                    },
                    new() {
                        Id = "autotest",
                        Name = "自动化测试",
                        Icon = "🧪",
                        Url = "/autotest",
                        Target = "_self"
                    }
                ]
            },
            new() {
                CategoryName = "低代码平台",
                Apps =
                [
                    new() {
                        Id = "design-engine",
                        Name = "应用开发",
                        Icon = "🛠️",
                        Url = "/workbench",
                        Target = "_blank"
                    }
                ]
            }
        };
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
        foreach (var category in _categories ?? new List<AppCategoryInfo>())
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

/// <summary>
/// 应用菜单项
/// </summary>
public class AppMenuItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = "📄";
}
