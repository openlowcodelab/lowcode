using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.RenderEngineBase;

/// <summary>
/// 页面组件基类
/// </summary>
public abstract class RenderEnginePageComponentBase : LowCodePageComponentBase
{
    [Inject] protected ISessionStorageService SessionStorageService { get; set; }

    [Parameter]
    public string AppId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await SessionStorageService.SetAsync("appid", AppId);

        await base.OnInitializedAsync();
    }
}
