using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Components;

namespace H.LowCode.RenderEngine
{
    public interface IRender
    {
        RenderFragment Render(PageSchema jsonSchema, PageRenderType pageRenderType);
    }
}
