using H.LowCode.Schema;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.RenderEngine
{
    public interface IRender
    {
        RenderFragment Render(PageSchema jsonSchema, PageRenderType pageRenderType);
    }
}
