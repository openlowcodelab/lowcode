using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 组件基类
/// </summary>
public abstract class LowCodeComponentBase : Microsoft.AspNetCore.Components.ComponentBase
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

    protected Uri GetBaseUri()
    {
        return new Uri( NavigationManager.BaseUri);
    }

    protected void NavigateTo([StringSyntax("Uri")] string uri, bool forceLoad = false)
    {
        NavigationManager.NavigateTo(uri);
    }
}
