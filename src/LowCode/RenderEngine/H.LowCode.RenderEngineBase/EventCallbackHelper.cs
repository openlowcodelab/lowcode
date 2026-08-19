using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// EventCallback 动态创建辅助 - 平台层通用能力
/// 用于按目标组件参数类型（EventCallback / EventCallback&lt;T&gt;）动态创建事件回调
/// </summary>
internal static class EventCallbackHelper
{
    private static readonly MethodInfo _createWithActionMethod = ResolveCreateMethod(typeof(Action<>));
    private static readonly MethodInfo _createWithFuncTaskMethod = ResolveCreateMethod(typeof(Func<Task>));

    private static MethodInfo ResolveCreateMethod(Type secondParamType)
    {
        var methods = typeof(EventCallbackFactory).GetMethods()
            .Where(m => m.Name == "Create" && m.IsGenericMethod && m.GetParameters().Length == 2);

        foreach (var method in methods)
        {
            var paramType = method.GetParameters()[1].ParameterType;
            if (secondParamType.IsGenericTypeDefinition)
            {
                if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == secondParamType)
                    return method;
            }
            else if (paramType == secondParamType)
            {
                return method;
            }
        }

        throw new InvalidOperationException($"EventCallbackFactory.Create method not found for {secondParamType}");
    }

    /// <summary>
    /// 创建带参数的 EventCallback（用于 ValueChanged 等），参数值交给 handler 处理
    /// </summary>
    public static object? CreateWithArg(object receiver, Type callbackType, Action<object?> handler)
    {
        if (callbackType == typeof(EventCallback))
        {
            return EventCallback.Factory.Create(receiver, () => handler(null));
        }

        if (!callbackType.IsGenericType || callbackType.GetGenericTypeDefinition() != typeof(EventCallback<>))
            return null;

        var valueType = callbackType.GetGenericArguments()[0];
        var createTypedHandler = typeof(EventCallbackHelper)
            .GetMethod(nameof(CreateTypedAction), BindingFlags.Static | BindingFlags.NonPublic)!
            .MakeGenericMethod(valueType);
        var typedAction = createTypedHandler.Invoke(null, new object[] { handler });

        return _createWithActionMethod
            .MakeGenericMethod(valueType)
            .Invoke(EventCallback.Factory, new object[] { receiver, typedAction! });
    }

    private static Action<T> CreateTypedAction<T>(Action<object?> handler)
    {
        return value => handler(value);
    }

    /// <summary>
    /// 创建无参数的 EventCallback（用于 OnClick 等事件消费），事件参数被忽略
    /// </summary>
    public static object? CreateWithoutArg(object receiver, Type callbackType, Func<Task> handler)
    {
        if (callbackType == typeof(EventCallback))
        {
            return EventCallback.Factory.Create(receiver, handler);
        }

        if (!callbackType.IsGenericType || callbackType.GetGenericTypeDefinition() != typeof(EventCallback<>))
            return null;

        var valueType = callbackType.GetGenericArguments()[0];
        return _createWithFuncTaskMethod
            .MakeGenericMethod(valueType)
            .Invoke(EventCallback.Factory, new object[] { receiver, handler });
    }
}
