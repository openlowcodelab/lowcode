using H.LowCode.RenderEngine.Html.PageRender;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.Html
{
    public class HtmlRender : IRender
    {
        public RenderFragment Render(JSchema jsonSchema, PageRenderType pageRenderType)
        {
            RenderFragment renderFragment = null;
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
