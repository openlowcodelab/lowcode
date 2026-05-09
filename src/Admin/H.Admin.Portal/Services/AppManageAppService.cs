using H.Admin.AppDrawer;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Admin.Portal;

/// <summary>
/// 应用管理服务，负责应用的增删改查操作
/// </summary>
[RemoteService]
public class AppManageAppService : ApplicationService, IAppManageAppService
{
    private readonly string _jsonFilePath;
    private readonly AppStateManager _appStateManager;

    public AppManageAppService(string? jsonFilePath = null, AppStateManager? appStateManager = null)
    {
        _jsonFilePath = jsonFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Admin", "data", "apps.json");
        _appStateManager = appStateManager ?? new AppStateManager(jsonFilePath);
    }

    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    public Task<List<AppCategoryInfo>> GetAllCategoriesAsync()
    {
        return Task.FromResult(_appStateManager.GetAppCategories());
    }

    /// <summary>
    /// 获取所有应用分类（同步版本，供内部使用）
    /// </summary>
    public List<AppCategoryInfo> GetAllCategories()
    {
        return _appStateManager.GetAppCategories();
    }

    /// <summary>
    /// 获取所有应用（扁平化）
    /// </summary>
    public List<AppItemInfo> GetAllApps()
    {
        var categories = GetAllCategories();
        var allApps = new List<AppItemInfo>();
        
        foreach (var category in categories)
        {
            if (category.Apps != null)
            {
                allApps.AddRange(category.Apps);
            }
        }
        
        return allApps.OrderBy(a => a.Order).ToList();
    }

    /// <summary>
    /// 根据 ID 获取应用
    /// </summary>
    public AppItemInfo? GetAppById(string appId)
    {
        var categories = GetAllCategories();
        
        foreach (var category in categories)
        {
            var app = category.Apps?.FirstOrDefault(a => a.Id == appId);
            if (app != null)
            {
                return app;
            }
        }
        
        return null;
    }

    /// <summary>
    /// 添加新应用
    /// </summary>
    public async Task<bool> AddAppAsync(string categoryId, AppItemInfo app)
    {
        try
        {
            var appData = LoadAppData();
            var category = appData.Categories.FirstOrDefault(c => c.CategoryName == categoryId);
            
            if (category == null)
            {
                // 创建新分类
                category = new AppCategoryInfo
                {
                    CategoryName = categoryId,
                    Apps = new List<AppItemInfo>()
                };
                appData.Categories.Add(category);
            }
            
            if (category.Apps == null)
            {
                category.Apps = new List<AppItemInfo>();
            }
            
            // 检查 ID 是否已存在
            if (category.Apps.Any(a => a.Id == app.Id))
            {
                throw new Exception($"应用 ID '{app.Id}' 已存在");
            }
            
            category.Apps.Add(app);
            
            await SaveAppDataAsync(appData);
            _appStateManager.ReloadApps();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"添加应用失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 更新应用
    /// </summary>
    public async Task<bool> UpdateAppAsync(string appId, AppItemInfo updatedApp)
    {
        try
        {
            var appData = LoadAppData();
            
            foreach (var category in appData.Categories)
            {
                var appIndex = category.Apps?.FindIndex(a => a.Id == appId) ?? -1;
                
                if (appIndex >= 0 && category.Apps != null)
                {
                    // 保留原有的菜单项（如果没有提供新的）
                    if (updatedApp.MenuItems == null || updatedApp.MenuItems.Count == 0)
                    {
                        updatedApp.MenuItems = category.Apps[appIndex].MenuItems;
                    }
                    
                    category.Apps[appIndex] = updatedApp;
                    
                    await SaveAppDataAsync(appData);
                    _appStateManager.ReloadApps();
                    
                    return true;
                }
            }
            
            throw new Exception($"应用 ID '{appId}' 不存在");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新应用失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    public async Task<bool> DeleteAppAsync(string appId)
    {
        try
        {
            var appData = LoadAppData();
            
            foreach (var category in appData.Categories)
            {
                var app = category.Apps?.FirstOrDefault(a => a.Id == appId);
                
                if (app != null && category.Apps != null)
                {
                    category.Apps.Remove(app);
                    
                    await SaveAppDataAsync(appData);
                    _appStateManager.ReloadApps();
                    
                    return true;
                }
            }
            
            throw new Exception($"应用 ID '{appId}' 不存在");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除应用失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 添加分类
    /// </summary>
    public async Task<bool> AddCategoryAsync(string categoryName)
    {
        try
        {
            var appData = LoadAppData();
            
            if (appData.Categories.Any(c => c.CategoryName == categoryName))
            {
                throw new Exception($"分类 '{categoryName}' 已存在");
            }
            
            appData.Categories.Add(new AppCategoryInfo
            {
                CategoryName = categoryName,
                Apps = new List<AppItemInfo>()
            });
            
            await SaveAppDataAsync(appData);
            _appStateManager.ReloadApps();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"添加分类失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除分类
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(string categoryName)
    {
        try
        {
            var appData = LoadAppData();
            var category = appData.Categories.FirstOrDefault(c => c.CategoryName == categoryName);
            
            if (category == null)
            {
                throw new Exception($"分类 '{categoryName}' 不存在");
            }
            
            if (category.Apps != null && category.Apps.Count > 0)
            {
                throw new Exception($"分类 '{categoryName}' 下还有应用，无法删除");
            }
            
            appData.Categories.Remove(category);
            
            await SaveAppDataAsync(appData);
            _appStateManager.ReloadApps();
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除分类失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 加载 JSON 数据
    /// </summary>
    private AppData LoadAppData()
    {
        if (!File.Exists(_jsonFilePath))
        {
            return new AppData();
        }
        
        var jsonContent = File.ReadAllText(_jsonFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        
        var appData = JsonSerializer.Deserialize<AppData>(jsonContent, options);
        return appData ?? new AppData();
    }

    /// <summary>
    /// 保存 JSON 数据
    /// </summary>
    private async Task SaveAppDataAsync(AppData appData)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        var jsonContent = JsonSerializer.Serialize(appData, options);
        
        // 确保目录存在
        var directory = Path.GetDirectoryName(_jsonFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(_jsonFilePath, jsonContent);
    }
}
