using AntDesign;
using H.LowCode.ComponentBase;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.RenderEngineBase;

public abstract class RenderEngineLowCodeComponentBase : LowCodeComponentBase
{
    [CascadingParameter(Name = "pageCascading")]
    public PageCascadingModel PageCascading { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
