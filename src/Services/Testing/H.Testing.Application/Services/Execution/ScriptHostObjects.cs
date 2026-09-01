namespace H.Testing.Application;

/// <summary>
/// 脚本步骤的 console 对象（JavaScript 脚本中可用 console.log 等）
/// </summary>
public class ScriptConsole
{
    private readonly Action<object?> _write;

    public ScriptConsole(Action<object?> write)
    {
        _write = write;
    }

    public void log(object? message) => _write(message);

    public void info(object? message) => _write(message);

    public void warn(object? message) => _write(message);

    public void error(object? message) => _write(message);
}

/// <summary>
/// C# 脚本的全局对象：暴露执行变量、环境配置、脚本参数与日志函数
/// </summary>
public class CSharpScriptGlobals
{
    /// <summary>执行变量（读写，修改会供后续步骤使用）</summary>
    public Dictionary<string, string> Vars { get; init; } = new();

    /// <summary>环境配置</summary>
    public Dictionary<string, object> Env { get; init; } = new();

    /// <summary>脚本参数</summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>输出到步骤日志</summary>
    public Action<object?> Log { get; init; } = _ => { };
}
