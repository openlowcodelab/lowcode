using H.AppDrawer.Components;
using H.SystemPortal.Application.Contracts;
using H.Util.Base;
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
    /// 定位 apps.json 文件：宿主可能从项目目录(dotnet run)或输出目录(直接运行 exe)启动，
    /// 仅依赖工作目录会静默取不到应用列表，故按候选位置逐级向上探测。
    /// 与 AppQueryAppService 使用同一套规则，保证管理页读写的文件与应用抽屉读到的是同一份
    /// </summary>
    private static string FindAppsJsonFile()
    {
        var startDirectories = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };

        // 发布布局：data 目录与可执行文件同级
        foreach (var start in startDirectories)
        {
            var besideExecutable = Path.Combine(start, "data", "apps.json");
            if (File.Exists(besideExecutable))
            {
                return besideExecutable;
            }
        }

        // 仓库布局：向上查找 System/SystemPortal/data/apps.json
        foreach (var start in startDirectories)
        {
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
            {
                var inRepository = Path.Combine(dir.FullName, "System", "SystemPortal", "data", "apps.json");
                if (File.Exists(inRepository))
                {
                    return inRepository;
                }
            }
        }

        return Path.Combine(Directory.GetCurrentDirectory(), "System", "SystemPortal", "data", "apps.json");
    }

    /// <summary>
    /// 获取所有应用分类
    /// </summary>
    public async Task<BaseOutput<AppCategoryInfo[]>> GetAllCategoriesAsync()
    {
        return new(LoadAppsFromJson());
    }

    /// <summary>
    /// 添加新应用
    /// </summary>
    public async Task<BaseOutput<bool>> AddAppAsync(string categoryId, AppItemInfo app)
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

            return new(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"添加应用失败: {ex.Message}");
            return new(false);
        }
    }

    /// <summary>
    /// 更新应用
    /// </summary>
    public async Task<BaseOutput<bool>> UpdateAppAsync(string appId, AppItemInfo updatedApp)
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

                    return new(true);
                }
            }

            throw new Exception($"应用 ID '{appId}' 不存在");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"更新应用失败: {ex.Message}");
            return new(false);
        }
    }

    /// <summary>
    /// 删除应用
    /// </summary>
    public async Task<BaseOutput<bool>> DeleteAppAsync(string appId)
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

                    return new(true);
                }
            }

            throw new Exception($"应用 ID '{appId}' 不存在");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除应用失败: {ex.Message}");
            return new(false);
        }
    }

    /// <summary>
    /// 添加分类
    /// </summary>
    public async Task<BaseOutput<bool>> AddCategoryAsync(string categoryName)
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

            return new(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"添加分类失败: {ex.Message}");
            return new(false);
        }
    }

    /// <summary>
    /// 删除分类
    /// </summary>
    public async Task<BaseOutput<bool>> DeleteCategoryAsync(string categoryName)
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

            return new(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除分类失败: {ex.Message}");
            return new(false);
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
                Console.WriteLine($"未找到 apps.json，应用列表为空: {_jsonFilePath}");
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
