using H.LowCode.RenderEngine.Html.PageRenders;
using Microsoft.AspNetCore.Components;
using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Html.Renders
{
    public class HtmlRender : IRender
    {
        public RenderFragment Render(PageSchema jsonSchema, PageRenderType pageRenderType)
        {
            RenderFragment renderFragment = null;
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
