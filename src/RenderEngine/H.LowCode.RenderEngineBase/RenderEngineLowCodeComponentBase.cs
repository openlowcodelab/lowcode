using AntDesign;
using H.LowCode.Application.Contracts;
using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.RenderEngineBase;

public abstract class RenderEngineLowCodeComponentBase : LowCodeComponentBase
{
    [CascadingParameter(Name = "pageCascading")]
    public PageCascadingModel PageCascading { get; set; }

    [Inject] protected new IMessageService Message { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
