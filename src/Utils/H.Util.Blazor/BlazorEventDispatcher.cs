namespace H.Util.Blazor;

public static class BlazorEventDispatcher
{
    /// <summary>
    /// key 格式建议(小写)：{组件库名称}.{组件名称}.{事件名称}
    /// 如：designengine.dragitem.onclick
    /// </summary>
    private static Dictionary<string, Action<object?>> _actions;
    static BlazorEventDispatcher()
    {
        _actions = [];
    }

    public static void Subscribe(string eventName, Action<object?> action)
    {
        if (!_actions.ContainsKey(eventName))
        {
            _actions.Add(eventName, action);
        }
        else
        {
            _actions[eventName] += action;
        }
    }

    public static void Remove(string eventName)
    {
        if (_actions.ContainsKey(eventName))
        {
            _actions.Remove(eventName);
        }
    }

    public static void Publish(string eventName, object? param)
    {
        if (_actions.ContainsKey(eventName))
        {
            var action = _actions[eventName];
            action.Invoke(param);
        }
        else
        {
            throw new Exception($"event name [{eventName}] is not exist");
        }
    }
}
