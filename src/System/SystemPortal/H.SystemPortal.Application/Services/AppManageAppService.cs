using H.AppDrawer.Components;
using H.SystemPortal.Application.Contracts;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.SystemPortal.Application;

/// <summary>
/// 应用管理服务，负责应用的查询和增删改操作
/// </summary>
[RemoteService]
public class AppManageAppService : ApplicationService, IAppManageAppService
{
    private readonly string _jsonFilePath;

    public AppManageAppService()
    {
        _jsonFilePath = FindAppsJsonFile();
    }

    /// <summary>
    /// 查找 apps.json 文件
    /// </summary>
    private string FindAppsJsonFile()
    {
#if DEBUG
        var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "System", "SystemPortal", "data", "apps.json");
        return jsonFilePath;
#else
        var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "apps.json");
        return jsonFilePath;
#endif
    }

    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    public async Task<AppCategoryInfo[]> GetAllCategoriesAsync()
    {
        return LoadAppsFromJson();
    }

    /// <summary>
    /// 添加新应用
    /// </summary>
    public async Task<bool> AddAppAsync(string categoryId, AppItemInfo app)
    {
        try
        {
            var appData = LoadAppData();
            var category = appData.AppCategories.FirstOrDefault(c => c.CategoryName == categoryId);

            if (category == null)
            {
                // 创建新分类
                category = new AppCategoryInfo
                {
                    CategoryName = categoryId,
                    Apps = []
                };
                appData.AppCategories.Add(category);
            }

            if (category.Apps == null)
            {
                category.Apps = [];
            }

            // 检查 ID 是否已存在
            if (category.Apps.Any(a => a.Id == app.Id))
            {
                throw new Exception($"应用 ID '{app.Id}' 已存在");
            }

            category.Apps.Add(app);

            await SaveAppDataAsync(appData);

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

            foreach (var category in appData.AppCategories)
            {
                var appIndex = category.Apps?.FindIndex(a => a.Id == appId) ?? -1;

                if (appIndex >= 0 && category.Apps != null)
                {
                    category.Apps[appIndex] = updatedApp;

                    await SaveAppDataAsync(appData);

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

            foreach (var category in appData.AppCategories)
            {
                var app = category.Apps?.FirstOrDefault(a => a.Id == appId);

                if (app != null && category.Apps != null)
                {
                    category.Apps.Remove(app);

                    await SaveAppDataAsync(appData);

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

            if (appData.AppCategories.Any(c => c.CategoryName == categoryName))
            {
                throw new Exception($"分类 '{categoryName}' 已存在");
            }

            appData.AppCategories.Add(new AppCategoryInfo
            {
                CategoryName = categoryName,
                Apps = new List<AppItemInfo>()
            });

            await SaveAppDataAsync(appData);

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
            var category = appData.AppCategories.FirstOrDefault(c => c.CategoryName == categoryName);

            if (category == null)
            {
                throw new Exception($"分类 '{categoryName}' 不存在");
            }

            if (category.Apps != null && category.Apps.Count > 0)
            {
                throw new Exception($"分类 '{categoryName}' 下还有应用，无法删除");
            }

            appData.AppCategories.Remove(category);

            await SaveAppDataAsync(appData);

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

        var appData = jsonContent.FromJson<AppData>();
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

    /// <summary>
    /// 从 JSON 文件加载应用数据
    /// </summary>
    private AppCategoryInfo[] LoadAppsFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                return [];
            }

            var jsonContent = File.ReadAllText(_jsonFilePath);
            var appData = jsonContent.FromJson<AppData>();

            return appData?.AppCategories?.ToArray() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load apps from JSON: {ex.Message}");
            return [];
        }
    }
}
