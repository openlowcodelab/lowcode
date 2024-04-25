using Microsoft.AspNetCore.Components;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.AntBlazor.PageRenders;

namespace H.LowCode.RenderEngine.AntBlazor.Renders
{
    public class AntBlazorRender : IRender
    {
        public RenderFragment Render(PageSchema jsonSchema, PageRenderType pageRenderType)
        {
            RenderFragment renderFragment;
            switch (pageRenderType)
            {
                case PageRenderType.Form:
                    FormRender render = new();
                    renderFragment = render.Render(jsonSchema);
                    break;
                default:
                    throw new NotSupportedException("");
            }
            return renderFragment;
        }
    }
}
