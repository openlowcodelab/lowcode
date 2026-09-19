using System.ComponentModel;
using System.Reflection;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 内置技能注册表的唯一真源：技能名 -> 实现类。
/// Seeder、SkillAppService 与 ToolRegistry 共用，避免"技能指向不存在的类"的幽灵注册。
/// </summary>
public static class WorkbenchToolCatalog
{
    /// <summary>
    /// MCP 工具的固定归属名，员工级工具隔离时始终放行
    /// </summary>
    public const string McpOwner = "mcp";

    public static readonly IReadOnlyDictionary<string, string> BuiltinSkillClasses =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["browser"] = "H.Workbench.Core.Tools.BrowserTool",
            ["search"] = "H.Workbench.Core.Tools.SearchTool",
            ["database"] = "H.Workbench.Core.Tools.DbTool",
            ["http_client"] = "H.Workbench.Core.Tools.HttpClientTool",
            ["git"] = "H.Workbench.Core.Tools.GitTool",
            ["workspace_file"] = "H.Workbench.Core.Tools.WorkspaceFileTool"
        };

    /// <summary>
    /// 解析实现类：Type.GetType 失败后扫描已加载程序集兜底（支持跨程序集实现类）
    /// </summary>
    public static Type? Resolve(string? implementationClass)
    {
        if (string.IsNullOrWhiteSpace(implementationClass)) return null;

        var type = Type.GetType(implementationClass, throwOnError: false);
        if (type != null) return type;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(implementationClass, throwOnError: false);
            if (type != null) return type;
        }

        return null;
    }

    /// <summary>
    /// 技能名 -> 内置实现类型（仅目录中登记且可加载的类型）
    /// </summary>
    public static IEnumerable<(string SkillName, Type Type)> GetBuiltinToolTypes()
    {
        foreach (var (skillName, className) in BuiltinSkillClasses)
        {
            var type = Resolve(className);
            if (type != null) yield return (skillName, type);
        }
    }

    /// <summary>
    /// 类型中所有可注册为工具的方法名（public + [Description]，排除 object 基类方法）
    /// </summary>
    public static IReadOnlyList<string> GetToolNames(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<DescriptionAttribute>() != null
                        && m.DeclaringType != typeof(object))
            .Select(m => m.Name)
            .ToList();
    }

    /// <summary>
    /// 校验实现类可加载且至少含一个工具方法；供 Seeder 与技能管理 UI 共用
    /// </summary>
    public static bool TryValidate(string? implementationClass,
        out IReadOnlyList<string> toolNames, out string? error)
    {
        toolNames = Array.Empty<string>();
        error = null;

        if (string.IsNullOrWhiteSpace(implementationClass))
        {
            error = "未指定实现类";
            return false;
        }

        var type = Resolve(implementationClass);
        if (type == null)
        {
            error = $"类型 {implementationClass} 无法加载";
            return false;
        }

        var names = GetToolNames(type);
        if (names.Count == 0)
        {
            error = $"类型 {type.Name} 没有带 [Description] 的可注册方法";
            return false;
        }

        toolNames = names;
        return true;
    }
}
