using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class InputRender : ElementRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.String)
                return false;

            jsonSchema.ExtensionData.TryGetValue("widget", out var widget);
            if (string.IsNullOrEmpty(jsonSchema.Format) && widget == null)
                return true;
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");          
            builder.AddContent(2, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenComponent(0, typeof(Input<string>));
            builder.AddAttribute(1, "Placeholder", jsonSchema.Title);
            if(jsonSchema.ExtensionData.TryGetValue("required", out var required))
                builder.AddAttribute(2, "required", "required");
            builder.CloseComponent();
        }
    }
}
