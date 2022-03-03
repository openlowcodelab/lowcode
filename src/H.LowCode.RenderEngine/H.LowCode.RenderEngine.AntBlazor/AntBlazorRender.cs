using H.LowCode.RenderEngine.AntBlazor.PageRender;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Schema;
using System;

namespace H.LowCode.RenderEngine.AntBlazor
{
    public class AntBlazorRender : IRender
    {
        public RenderFragment Render(JSchema jsonSchema, PageRenderType pageRenderType)
        {
            RenderFragment renderFragment;
            switch (pageRenderType)
            {
                case PageRenderType.Form:
                    FormRender render = new FormRender();
                    renderFragment = render.Render(jsonSchema);
                    break;
                default:
                    throw new NotSupportedException("");
            }
            return renderFragment;
        }
    }
}
