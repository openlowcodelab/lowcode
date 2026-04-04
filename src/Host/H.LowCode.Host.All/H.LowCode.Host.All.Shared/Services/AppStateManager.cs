using H.Util.Blazor;

namespace H.LowCode.Host.All.Shared.Services;

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
        return new List<AppCategoryInfo>
        {
            new AppCategoryInfo
            {
                CategoryName = "基础服务",
                Apps = new List<AppItemInfo>
                {
                    new AppItemInfo { 
                        Id = "account", 
                        Name = "用户管理", 
                        Icon = "👤", 
                        Url = "/account", 
                        Target = "_self" 
                    },
                    new AppItemInfo { 
                        Id = "organization", 
                        Name = "组织管理", 
                        Icon = "🏢", 
                        Url = "/organization", 
                        Target = "_self" 
                    }
                }
            },
            new AppCategoryInfo
            {
                CategoryName = "低代码平台",
                Apps = new List<AppItemInfo>
                {
                    new AppItemInfo { 
                        Id = "design-engine", 
                        Name = "应用设计器", 
                        Icon = "🛠️", 
                        Url = "/design", 
                        Target = "_self" 
                    },
                    new AppItemInfo { 
                        Id = "render-engine", 
                        Name = "应用预览", 
                        Icon = "👁️", 
                        Url = "/render", 
                        Target = "_self" 
                    }
                }
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
            _ => "portal-menu"
        };
    }
}
