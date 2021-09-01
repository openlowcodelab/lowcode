using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class SelectRender : ElementRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.String)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "select", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string keys, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenElement(10, "div");
            builder.AddAttribute(11, "class", "");
            builder.AddContent(12, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenComponent(10, typeof(Select<string, string>));
            builder.CloseComponent();
        }
    }
}
