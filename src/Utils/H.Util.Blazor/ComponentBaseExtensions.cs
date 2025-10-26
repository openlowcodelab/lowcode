using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Util.Blazor;

public static class ComponentBaseExtensions
{
    public static async Task RedirectPageAsync(this ComponentBase component, string url, string target = "_blank")
    {
        var jsRuntime = ServiceLocator.GetService<IJSRuntime>();
        await jsRuntime.InvokeVoidAsync("open", url, target);
    }
}
