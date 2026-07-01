using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using H.Assistant.Application.Contracts;
using H.Assistant.Core.Tools;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace H.Assistant.Core;

/// <summary>
/// 工具注册中心实现
/// </summary>
public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, AIFunction> _tools = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<ToolRegistry> _logger;

    public ToolRegistry(ILogger<ToolRegistry> logger)
    {
        _logger = logger;
        RegisterBuiltinTools();
    }

    /// <summary>
    /// 注册内置工具（反射扫描带 [Description] 的 public static 方法）
    /// </summary>
    private void RegisterBuiltinTools()
    {
        var toolTypes = new[]
        {
            typeof(BrowserTool),
            typeof(SearchTool),
            typeof(DbTool),
            typeof(HttpClientTool)
        };

        foreach (var type in toolTypes)
        {
            RegisterToolsFromType(type);
        }
    }

    /// <summary>
    /// 从类型中扫描所有带 [Description] 的 public static 方法并注册
    /// </summary>
    private void RegisterToolsFromType(Type type)
    {
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        // 提供实例创建工厂（AIFunctionFactory 要求此参数非 null，即使方法为 static）
        Func<IServiceProvider, object?> createInstance = _ => Activator.CreateInstance(type);

        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr == null) continue;

            // 跳过 object 基类方法
            if (method.DeclaringType == typeof(object)) continue;

            try
            {
                var aiFunction = AIFunctionFactory.Create(method, createInstance, new AIFunctionFactoryOptions
                {
                    Name = method.Name,
                    Description = descAttr.Description
                });

                _tools[aiFunction.Name] = aiFunction;
                _logger.LogDebug("注册内置工具: {ToolName}", aiFunction.Name);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "注册工具 {TypeName}.{MethodName} 失败", type.Name, method.Name);
            }
        }
    }

    /// <summary>
    /// 从技能定义注册工具（扫描所有带 [Description] 的 public 方法）
    /// </summary>
    public void RegisterSkillTools(List<SkillDto> skills)
    {
        foreach (var skill in skills.Where(s => s.IsEnabled && !string.IsNullOrWhiteSpace(s.ImplementationClass)))
        {
            try
            {
                var type = Type.GetType(skill.ImplementationClass);
                if (type == null)
                {
                    _logger.LogWarning("技能 {SkillName} 的类型 {ClassName} 无法加载", skill.SkillName, skill.ImplementationClass);
                    continue;
                }

                // 扫描所有带 [Description] 的 public 方法（包括 static 和 instance）
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
                bool registered = false;

                // 提供实例创建工厂（AIFunctionFactory 要求此参数非 null）
                Func<IServiceProvider, object?> createInstance = _ =>
                {
                    try { return Activator.CreateInstance(type); }
                    catch { return null; }
                };

                foreach (var method in methods)
                {
                    var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr == null) continue;
                    if (method.DeclaringType == typeof(object)) continue;

                    try
                    {
                        var aiFunction = AIFunctionFactory.Create(method, createInstance, new AIFunctionFactoryOptions
                        {
                            Name = method.Name,
                            Description = descAttr.Description
                        });

                        _tools[aiFunction.Name] = aiFunction;
                        registered = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "注册技能方法 {TypeName}.{MethodName} 失败", type.Name, method.Name);
                    }
                }

                if (!registered)
                {
                    _logger.LogWarning("技能 {SkillName} 没有找到可注册的方法", skill.SkillName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "注册技能 {SkillName} 失败", skill.SkillName);
            }
        }
    }

    /// <summary>
    /// 注册 MCP 工具（从外部传入的 AIFunction）
    /// </summary>
    public void RegisterMcpTool(AIFunction tool)
    {
        _tools[tool.Name] = tool;
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
        return _tools.Values.Select(f =>
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
        }).ToList();
    }

    private static string MapClrTypeToJsonType(Type? type)
    {
        if (type == null) return "string";

        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string)) return "string";
        if (underlying == typeof(int) || underlying == typeof(long) ||
            underlying == typeof(short) || underlying == typeof(byte)) return "integer";
        if (underlying == typeof(float) || underlying == typeof(double) ||
            underlying == typeof(decimal)) return "number";
        if (underlying == typeof(bool)) return "boolean";
        if (underlying.IsArray || (underlying.IsGenericType &&
            underlying.GetGenericTypeDefinition() == typeof(List<>))) return "array";
        if (underlying.IsGenericType &&
            underlying.GetGenericTypeDefinition() == typeof(IDictionary<,>)) return "object";
        if (underlying == typeof(IDictionary<string, string>)) return "object";

        return "string";
    }
}
