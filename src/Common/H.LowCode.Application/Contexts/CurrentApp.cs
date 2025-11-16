using H.LowCode.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace H.LowCode.Application;

/// <summary>
/// 应用上下文服务实现
/// 负责从 HTTP 上下文中解析和管理当前请求的 AppId
/// </summary>
public class CurrentApp : ICurrentApp
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentApp> _logger;
    private string? _currentAppId;

    public CurrentApp(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentApp> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前请求的 AppId
    /// </summary>
    public string? CurrentAppId
    {
        get
        {
            if (_currentAppId == null)
            {
                ResolveAppIdFromContext();
            }
            return _currentAppId;
        }
    }

    /// <summary>
    /// 设置当前请求的 AppId
    /// </summary>
    /// <param name="appId">应用ID</param>
    public void SetAppId(string? appId)
    {
        _currentAppId = appId;
        _logger.LogDebug("AppId set to: {AppId}", appId);
    }

    /// <summary>
    /// 从当前 HTTP 上下文中解析 AppId
    /// 
    /// 解析优先级（从高到低）：
    /// 1. 请求头：x-appid - 最高优先级，适用于 API 调用和组件间通信
    /// 2. 查询字符串：?appId=xxx - 中等优先级，适用于直接 URL 访问
    /// 3. URL 路径解析 - 最低优先级，作为后备方案
    /// 
    /// 支持的路由模式：
    /// 1. 设计引擎路由：/designer/{AppId}/{PageId} - AppId 位于第二个路径段
    /// 2. 应用管理路由：/myapp/{AppId}/... - AppId 位于第二个路径段  
    /// 3. 渲染引擎路由：/{AppId}/... - AppId 位于第一个路径段
    /// 
    /// 系统路径（不包含 AppId）：
    /// - 根路径：/
    /// - 应用列表：/myapps
    /// - 解决方案：/solutions
    /// - 文档：/docs
    /// - 物料管理：/myparts/*
    /// - API 路径：/api/*
    /// - 静态资源：包含文件扩展名的路径
    /// - Blazor 框架：/_framework/*
    /// 
    /// AppId 验证规则：
    /// - 不能为空或空白字符串
    /// - 不能包含文件扩展名（不能包含 '.' 字符）
    /// - 长度必须大于 0
    /// </summary>
    public void ResolveAppIdFromContext()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext is null, cannot resolve AppId");
                return;
            }

            // 优先级 1: 尝试从请求头中获取 AppId (最高优先级)
            if (httpContext.Request.Headers.TryGetValue("x-appid", out var headerAppId))
            {
                var appId = headerAppId.ToString();
                if (!string.IsNullOrEmpty(appId))
                {
                    _currentAppId = appId;
                    _logger.LogDebug("AppId resolved from header: {AppId}", appId);
                    return;
                }
            }

            // 优先级 2: 尝试从查询字符串中获取 AppId
            if (httpContext.Request.Query.TryGetValue("appId", out var queryAppId))
            {
                var appId = queryAppId.ToString();
                if (!string.IsNullOrEmpty(appId))
                {
                    _currentAppId = appId;
                    _logger.LogDebug("AppId resolved from query string: {AppId}", appId);
                    return;
                }
            }

            // 优先级 3: 尝试从 URL 路径中解析 AppId (最低优先级，作为后备方案)
            var path = httpContext.Request.Path.Value;
            if (!string.IsNullOrEmpty(path))
            {
                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 0)
                {
                    var appId = ResolveAppIdFromPathSegments(segments);
                    if (!string.IsNullOrEmpty(appId))
                    {
                        _currentAppId = appId;
                        _logger.LogDebug("AppId resolved from URL path: {AppId} (path: {Path})", appId, path);
                        return;
                    }
                }
            }

            _logger.LogDebug("AppId could not be resolved from context, path: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving AppId from context");
        }
    }

    /// <summary>
    /// 从路径段中解析 AppId
    /// </summary>
    /// <summary>
    /// 从路径段中解析 AppId
    /// </summary>
    /// <param name="pathSegments">路径段数组</param>
    /// <returns>解析出的 AppId，如果无法解析则返回 null</returns>
    private string? ResolveAppIdFromPathSegments(string[] pathSegments)
    {
        if (pathSegments == null || pathSegments.Length == 0)
            return null;

        // 模式 1: /designer/{AppId}/{PageId} -> ["designer", "AppId", "PageId"]
        if (pathSegments.Length >= 2 && pathSegments[0].Equals("designer", StringComparison.OrdinalIgnoreCase))
        {
            var appId = pathSegments[1];
            return IsValidAppId(appId) ? appId : null;
        }

        // 模式 2: /myapp/{AppId}/... -> ["myapp", "AppId", ...]
        if (pathSegments.Length >= 2 && pathSegments[0].Equals("myapp", StringComparison.OrdinalIgnoreCase))
        {
            var appId = pathSegments[1];
            return IsValidAppId(appId) ? appId : null;
        }

        // 特殊系统路径检查 - 这些路径不包含 AppId
        if (pathSegments.Length >= 1)
        {
            var firstSegment = pathSegments[0];
            
            // /myparts/... - 物料管理路径，不包含 AppId
            if (firstSegment.Equals("myparts", StringComparison.OrdinalIgnoreCase))
                return null;
                
            // /api/... - API 路径，不包含 AppId
            if (firstSegment.Equals("api", StringComparison.OrdinalIgnoreCase))
                return null;
        }

        // 模式 3: /{AppId}/... (渲染引擎路由) -> ["AppId", ...]
        if (pathSegments.Length >= 1)
        {
            var firstSegment = pathSegments[0];
            
            // 排除系统路径
            if (IsSystemPath(firstSegment))
                return null;
                
            return IsValidAppId(firstSegment) ? firstSegment : null;
        }

        return null;
    }

    /// <summary>
    /// 判断是否为系统路径（不包含 AppId）
    /// 
    /// 系统路径包括：
    /// - API 路径：api
    /// - 应用列表：myapps
    /// - 解决方案：solutions
    /// - 文档：docs
    /// - 生态系统：ecosystems
    /// - 物料管理：myparts
    /// - 错误页面：error
    /// - Blazor 框架：_framework, _content
    /// - 静态资源：css, js, images, fonts
    /// </summary>
    /// <param name="segment">路径段</param>
    /// <returns>是否为系统路径</returns>
    private bool IsSystemPath(string segment)
    {
        var systemPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "css",          // 样式文件
            "js",           // JavaScript 文件
            "images",       // 图片文件
            "fonts",        // 字体文件
            "favicon.ico",   // 网站图标
            "_framework",   // Blazor 框架路径
            "_blazor",
            "_content",     // 静态内容路径
            "error",        // 错误页面
            "myapps",       // 应用列表
            "solutions",    // 解决方案
            "docs",         // 文档
            "ecosystems",   // 生态系统
            "myparts",      // 物料管理
            "api",          // API 路径
        };

        return systemPaths.Contains(segment);
    }

    /// <summary>
    /// 验证是否为有效的 AppId
    /// 
    /// AppId 验证规则：
    /// 1. 不能为空或空白字符串
    /// 2. 不能包含文件扩展名（不能包含 '.' 字符）
    /// 3. 只能包含字母、数字、下划线(_)、连字符(-)
    /// 4. 用于区分应用标识符和静态资源文件
    /// 
    /// 有效的 AppId 示例：
    /// - "caseapp"
    /// - "test-app"
    /// - "my_application"
    /// - "app123"
    /// 
    /// 无效的 AppId 示例：
    /// - "app.css" (包含扩展名)
    /// - "app/page" (包含特殊字符)
    /// - "" (空字符串)
    /// </summary>
    /// <param name="segment">路径段</param>
    /// <returns>是否为有效的 AppId</returns>
    private bool IsValidAppId(string segment)
    {
        // AppId 应该是非空字符串，且不包含特殊字符
        if (string.IsNullOrEmpty(segment))
            return false;

        // 排除文件扩展名（如 .css, .js 等）
        if (segment.Contains('.'))
            return false;

        // AppId 通常是字母、数字、下划线、连字符的组合
        return segment.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-');
    }
}