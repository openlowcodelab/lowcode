using System;

namespace H.Admin.AppDrawer;

/// <summary>
/// 应用状态管理器，负责管理当前应用状态和应用列表
/// </summary>
public class AppStateManager
{
    private string _currentAppId = "portal";

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
        return
        [
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
        ];
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
            _ => "portal-menu"
        };
    }

    /// <summary>
    /// 获取指定应用的菜单项列表
    /// </summary>
    public List<AppMenuItem> GetAppMenuItems(string appId)
    {
        return appId switch
        {
            "account" => GetAccountMenuItems(),
            "organization" => GetOrganizationMenuItems(),
            "design-engine" => GetDesignEngineMenuItems(),
            "render-engine" => GetRenderEngineMenuItems(),
            _ => new List<AppMenuItem>()
        };
    }

    /// <summary>
    /// Account 应用菜单
    /// </summary>
    private List<AppMenuItem> GetAccountMenuItems()
    {
        return new List<AppMenuItem>
        {
            new AppMenuItem { Name = "用户列表", Url = "/account/users", Icon = "👥" }
        };
    }

    /// <summary>
    /// Organization 应用菜单
    /// </summary>
    private List<AppMenuItem> GetOrganizationMenuItems()
    {
        return new List<AppMenuItem>
        {
            new AppMenuItem { Name = "组织管理", Url = "/organization", Icon = "🏢" },
            new AppMenuItem { Name = "成员管理", Url = "/organization/member", Icon = "👥" },
            new AppMenuItem { Name = "角色管理", Url = "/organization/role", Icon = "🔑" }
        };
    }

    /// <summary>
    /// DesignEngine 应用菜单
    /// </summary>
    private List<AppMenuItem> GetDesignEngineMenuItems()
    {
        return new List<AppMenuItem>
        {
            new AppMenuItem { Name = "我的应用", Url = "/myapps", Icon = "📱" },
            new AppMenuItem { Name = "页面管理", Url = "/pages", Icon = "📄" },
            new AppMenuItem { Name = "数据源管理", Url = "/datasources", Icon = "💾" }
        };
    }

    /// <summary>
    /// RenderEngine 应用菜单
    /// </summary>
    private List<AppMenuItem> GetRenderEngineMenuItems()
    {
        return new List<AppMenuItem>
        {
            new AppMenuItem { Name = "应用预览", Url = "/render", Icon = "👁️" },
            new AppMenuItem { Name = "主题管理", Url = "/themes", Icon = "🎨" }
        };
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
