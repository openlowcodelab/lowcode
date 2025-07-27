using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class LowCodePageComponentBase : LowCodeComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Logger.LogInformation($"渲染模式: {RendererInfo.Name}, path=/{NavigationManager.ToBaseRelativePath(NavigationManager.Uri)}");
    }

    protected static T GetQueryValue<T>(string name)
    {
        return default;
    }
}
