using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputCheckboxRender : ComponentRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.Boolean)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkbox", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            if (jsonSchema.Required.Count > 0)
            {
                builder.AddMarkupContent(2, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "checkbox");

            if (jsonSchema.Required.Count > 0)
                builder.AddAttribute(3, "required", "required");

            builder.CloseElement();
        }
    }
}
