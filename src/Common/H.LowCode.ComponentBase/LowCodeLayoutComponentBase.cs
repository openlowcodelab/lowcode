using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 布局组件基类
/// </summary>
public abstract class LowCodeLayoutComponentBase : LayoutComponentBase
{
    [Inject]
    private LowCodeAppState LowCodeAppState { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

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

    private string _appId;
    protected string AppId
    {
        get
        {
            if (_appId.IsNullOrEmpty())
            {
                _appId = GetRouteValue("AppId");
                return _appId;
            }
            return _appId;
        }
    }

    protected Uri GetBaseUri()
    {
        return new Uri(NavigationManager.BaseUri);
    }

    protected string GetRouteValue(string name)
    {
        string routeValue = string.Empty;
        if ((this.Body.Target as RouteView)?.RouteData.RouteValues?.TryGetValue(name, out object routeVal) == true)
        {
            routeValue = routeVal?.ToString() ?? string.Empty;
        }

        return routeValue;
    }
}
