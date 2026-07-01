using Microsoft.AspNetCore.Components;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class LowCodePageComponentBase : LowCodeComponentBase
{
    [Parameter] public string AppId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }

    protected static T GetQueryValue<T>(string name)
    {
        return default;
    }
}
