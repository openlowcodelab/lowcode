namespace H.LowCode.RenderEngineBase;

/// <summary>
/// 页面表单状态服务 - 平台层通用能力
/// 统一管理页面内所有输入组件的值，支持值变化通知（驱动显隐联动等重新求值）
/// </summary>
/// <remarks>
/// key 约定：
/// 普通组件为组件 Name；
/// 列表实例组件为 "{listId}|{itemPrimaryKey}|{componentName}"。
/// 特殊 key "__lastid" 保存最近一次表单保存生成的主键。
/// </remarks>
public class PageFormStateService
{
    /// <summary>
    /// 最近一次保存生成主键的状态 key
    /// </summary>
    public const string LastIdKey = "__lastid";

    private readonly Dictionary<string, object?> _values = new();

    /// <summary>
    /// 值变化通知
    /// </summary>
    public event Action? OnChange;

    public void SetValue(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            return;

        _values.TryGetValue(key, out var existing);
        if (Equals(existing, value))
            return;

        _values[key] = value;
        OnChange?.Invoke();
    }

    /// <summary>
    /// 静默设值（不触发通知，用于初始化回填）
    /// </summary>
    public void SetValueSilently(string key, object? value)
    {
        if (string.IsNullOrEmpty(key))
            return;

        _values[key] = value;
    }

    public object? GetValue(string key)
    {
        return _values.TryGetValue(key, out var value) ? value : null;
    }

    public bool HasValue(string key)
    {
        return _values.ContainsKey(key);
    }

    /// <summary>
    /// 获取所有状态值（只读快照）
    /// </summary>
    public IReadOnlyDictionary<string, object?> GetAllValues()
    {
        return _values;
    }

    /// <summary>
    /// 获取指定列表组件内指定组件名的全部实例值
    /// </summary>
    /// <param name="listId">列表组件 Id</param>
    /// <param name="componentName">组件名</param>
    /// <returns>key: 行主键, value: 组件值</returns>
    public IDictionary<string, object?> GetListInstanceValues(string listId, string componentName)
    {
        var prefix = $"{listId}|";
        var suffix = $"|{componentName}";

        var result = new Dictionary<string, object?>();
        foreach (var kv in _values)
        {
            if (!kv.Key.StartsWith(prefix) || !kv.Key.EndsWith(suffix))
                continue;

            var itemKey = kv.Key.Substring(prefix.Length,
                kv.Key.Length - prefix.Length - suffix.Length);
            result[itemKey] = kv.Value;
        }

        return result;
    }

    public void Clear()
    {
        _values.Clear();
    }
}
