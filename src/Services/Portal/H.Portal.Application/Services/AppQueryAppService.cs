using H.AppDrawer.Components;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Portal.Application;

/// <summary>
/// 应用查询服务（只读），负责从 JSON 文件读取应用数据
/// 前端通过 HttpClient 调用 /api/app/app-query/*（ABP 约定路由）
/// </summary>
[RemoteService]
public class AppQueryAppService : ApplicationService
{
    private readonly string _jsonFilePath;

    public AppQueryAppService()
    {
        _jsonFilePath = FindAppsJsonFile();
    }

    /// <summary>
    /// 定位 apps.json 文件：宿主可能从项目目录(dotnet run)或输出目录(直接运行 exe)启动，
    /// 仅依赖工作目录会静默取不到应用列表，故按候选位置逐级向上探测
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
    /// 获取所有应用（扁平化）
    /// </summary>
    public async Task<BaseOutput<AppItemInfo[]>> GetAllAppsAsync()
    {
        var categories = (await GetAllCategoriesAsync()).Data ?? [];
        var allApps = new List<AppItemInfo>();

        foreach (var category in categories)
        {
            if (category.Apps != null)
            {
                allApps.AddRange(category.Apps);
            }
        }

        return new(allApps.OrderBy(a => a.Order).ToArray());
    }

    /// <summary>
    /// 根据 ID 获取应用
    /// </summary>
    public async Task<BaseOutput<AppItemInfo?>> GetAppByIdAsync(string appId)
    {
        var categories = (await GetAllCategoriesAsync()).Data ?? [];

        foreach (var category in categories)
        {
            var app = category.Apps?.FirstOrDefault(a => a.Id == appId);
            if (app != null)
            {
                return new(app);
            }
        }

        return new(null);
    }

    /// <summary>
    /// 加载 JSON 数据
    /// </summary>
    protected AppData LoadAppData()
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
