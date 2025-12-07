using AntDesign;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.ComponentBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class LowCodePageComponentBase : LowCodeComponentBase
{
    [Parameter] public string AppId { get; set; }

    [Inject] protected ISessionStorageService SessionStorageService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(AppId))
        {
            await SessionStorageService.SetAsync("appid", AppId);
        }

        await base.OnInitializedAsync();
    }

    protected static T GetQueryValue<T>(string name)
    {
        return default;
    }
}
