using H.Workbench.Application.Contracts;
using H.Workbench.Core.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace H.Workbench.Core;

/// <summary>
/// 工具注册中心实现。
/// 注意：本类为 DI 单例，注册的工具实例只能依赖单例服务（IOptions/ILogger/单例锁），
/// 禁止经工具构造函数捕获 scoped 服务（AppService/Repository），否则会产生跨请求 UoW 复用。
/// </summary>
public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, AIFunction> _tools = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _toolOwners = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ToolRegistry> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ToolRegistry(ILogger<ToolRegistry> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        RegisterBuiltinTools();
    }

    /// <summary>
    /// 注册内置工具（类型来自 WorkbenchToolCatalog 唯一真源，归属登记为对应技能名）
    /// </summary>
    private void RegisterBuiltinTools()
    {
        foreach (var (skillName, type) in WorkbenchToolCatalog.GetBuiltinToolTypes())
        {
            RegisterToolsFromType(type, skillName);
        }
    }

    private int RegisterToolsFromType(Type type, string owner)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        var registered = 0;

        // 实例方法需要真实目标对象：已注册 DI 的类型解析为共享单例，否则新建实例。
        // 注意：本版本 AIFunctionFactory 只有 Create(MethodInfo, object target, options) 重载，
        // 传工厂委托会被当作 target 导致运行期 "does not match target type" 异常。
        object? instanceCache = null;
        object? GetTarget(MethodInfo method)
        {
            if (method.IsStatic) return null;
            return instanceCache ??= ActivatorUtilities.CreateInstance(_serviceProvider, type);
        }

        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr == null) continue;

            // 跳过 object 基类方法
            if (method.DeclaringType == typeof(object)) continue;

            try
            {
                var aiFunction = AIFunctionFactory.Create(method, GetTarget(method), new AIFunctionFactoryOptions
                {
                    Name = method.Name,
                    Description = descAttr.Description
                });

                _tools[aiFunction.Name] = aiFunction;
                _toolOwners[aiFunction.Name] = owner;
                registered++;
                _logger.LogDebug("注册工具: {ToolName} (归属 {Owner})", aiFunction.Name, owner);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "注册工具 {TypeName}.{MethodName} 失败", type.Name, method.Name);
            }
        }

        return registered;
    }

    /// <summary>
    /// 从技能定义注册工具（扫描所有带 [Description] 的 public 方法），返回注册结果供调用方感知失败
    /// </summary>
    public SkillRegistrationReport RegisterSkillTools(List<SkillDto> skills)
    {
        var ok = 0;
        var failed = 0;
        var errors = new List<string>();

        foreach (var skill in skills.Where(s => s.IsEnabled && !string.IsNullOrWhiteSpace(s.ImplementationClass)))
        {
            try
            {
                var type = WorkbenchToolCatalog.Resolve(skill.ImplementationClass);
                if (type == null)
                {
                    failed++;
                    errors.Add($"技能 {skill.SkillName} 的类型 {skill.ImplementationClass} 无法加载");
                    _logger.LogWarning("技能 {SkillName} 的类型 {ClassName} 无法加载", skill.SkillName, skill.ImplementationClass);
                    continue;
                }

                var registeredCount = RegisterToolsFromType(type, skill.SkillName);

                if (registeredCount > 0)
                {
                    ok++;
                }
                else
                {
                    failed++;
                    errors.Add($"技能 {skill.SkillName} 没有找到可注册的方法");
                    _logger.LogWarning("技能 {SkillName} 没有找到可注册的方法", skill.SkillName);
                }
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"技能 {skill.SkillName} 注册失败: {ex.Message}");
                _logger.LogWarning(ex, "注册技能 {SkillName} 失败", skill.SkillName);
            }
        }

        return new SkillRegistrationReport(ok, failed, errors);
    }

    /// <summary>
    /// 注册 MCP 工具（从外部传入的 AIFunction），归属固定为 "mcp"
    /// </summary>
    public void RegisterMcpTool(AIFunction tool)
    {
        _tools[tool.Name] = tool;
        _toolOwners[tool.Name] = WorkbenchToolCatalog.McpOwner;
        _logger.LogDebug("注册 MCP 工具: {ToolName}", tool.Name);
    }

    public IReadOnlyList<AIFunction> GetAllTools() => _tools.Values.ToList();

    public AIFunction? GetTool(string name)
    {
        _tools.TryGetValue(name, out var tool);
        return tool;
    }

    public List<ToolDefinition> GetToolDefinitions()
    {
        return _tools.Values.Select(ToDefinition).ToList();
    }

    /// <summary>
    /// 创建按归属技能过滤的工具视图；allowedOwners 为 null 时返回自身（全量视图）。
    /// 视图是动态的：注册发生在创建之后依然可见（只要归属在允许集合内）。
    /// </summary>
    public IToolRegistry CreateScoped(IReadOnlyCollection<string>? allowedOwners)
    {
        return allowedOwners == null
            ? this
            : new ScopedToolRegistry(this, new HashSet<string>(allowedOwners, StringComparer.OrdinalIgnoreCase));
    }

    internal bool TryGetTool(string name, out AIFunction? tool, out string? owner)
    {
        var found = _tools.TryGetValue(name, out tool);
        owner = found && _toolOwners.TryGetValue(name, out var o) ? o : null;
        return found;
    }

    internal IReadOnlyDictionary<string, string> ToolOwners => _toolOwners;

    private static ToolDefinition ToDefinition(AIFunction f)
    {
        var toolDef = new ToolDefinition
        {
            Type = "function",
            Function = new FunctionDefinition
            {
                Name = f.Name,
                Description = f.Description ?? string.Empty
            }
        };

        // 使用 AIFunction 提供的 JsonSchema 作为参数定义
        if (f.JsonSchema.ValueKind != JsonValueKind.Undefined)
        {
            toolDef.Function.Parameters = JsonSerializer.Deserialize<object>(f.JsonSchema.GetRawText());
        }

        return toolDef;
    }

    /// <summary>
    /// 员工级工具视图：只暴露归属在允许技能集合内的工具
    /// </summary>
    private sealed class ScopedToolRegistry : IToolRegistry
    {
        private readonly ToolRegistry _parent;
        private readonly HashSet<string> _allowedOwners;

        public ScopedToolRegistry(ToolRegistry parent, HashSet<string> allowedOwners)
        {
            _parent = parent;
            _allowedOwners = allowedOwners;
        }

        public IReadOnlyList<AIFunction> GetAllTools()
            => _parent._tools.Where(kv => IsAllowed(kv.Key)).Select(kv => kv.Value).ToList();

        public AIFunction? GetTool(string name)
            => _parent.TryGetTool(name, out var tool, out var owner) && owner != null && _allowedOwners.Contains(owner)
                ? tool
                : null;

        public List<ToolDefinition> GetToolDefinitions()
            => _parent._tools.Where(kv => IsAllowed(kv.Key)).Select(kv => ToDefinition(kv.Value)).ToList();

        public void RegisterMcpTool(AIFunction tool) => _parent.RegisterMcpTool(tool);

        public SkillRegistrationReport RegisterSkillTools(List<SkillDto> skills) => _parent.RegisterSkillTools(skills);

        public IToolRegistry CreateScoped(IReadOnlyCollection<string>? allowedOwners)
            => allowedOwners == null
                ? this
                : new ScopedToolRegistry(_parent, new HashSet<string>(allowedOwners, StringComparer.OrdinalIgnoreCase));

        private bool IsAllowed(string toolName)
            => _parent._toolOwners.TryGetValue(toolName, out var owner) && _allowedOwners.Contains(owner);
    }
}
