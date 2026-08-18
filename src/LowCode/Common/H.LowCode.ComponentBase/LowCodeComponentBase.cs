using H.Util.Blazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Diagnostics.CodeAnalysis;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 组件基类
/// </summary>
public abstract class LowCodeComponentBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Inject] private LowCodeAppState LowCodeAppState { get; set; }

    [Inject] protected NavigationManager NavigationManager { get; set; }

    [Inject] protected IJSRuntime JSRuntime { get; set; }

    [Inject] private ILoggerFactory LoggerFactory { get; set; }

    /// <summary>
    /// 日志记录器（按实际组件类型创建）
    /// </summary>
    protected ILogger Logger => LoggerFactory.CreateLogger(GetType());

    [Inject] protected HToastService Toast { get; set; }

    /// <summary>
    /// 组件状态标识 (用于 ShouldRender 判断)
    /// </summary>
    protected string StateKey { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected bool IsDesign
    {
        get
        {
            return LowCodeAppState.IsDesign;
        }
    }

    protected Uri GetBaseUri()
    {
        return new Uri(NavigationManager.BaseUri);
    }

    protected void NavigateTo([StringSyntax("Uri")] string uri, bool forceLoad = false)
    {
        NavigationManager.NavigateTo(uri, forceLoad);
    }

    protected string GetQueryValue(string key)
    {
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        return QueryHelpers.ParseQuery(uri.Query).GetValueOrDefault(key);
    }
}
