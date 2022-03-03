using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputTextAreaRender : ComponentRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.String)
                return false;

            if (string.Equals(jsonSchema.Format, "textarea", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            builder.AddAttribute(2, "style", "vertical-align: top;");
            if (jsonSchema.Required.Count > 0)
            {
                builder.AddMarkupContent(3, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(4, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "textarea");
            builder.AddAttribute(1, "class", "field-value");
            builder.AddAttribute(2, "style", "height:50px;");

            if (jsonSchema.Required.Count > 0)
                builder.AddAttribute(3, "required", "required");

            builder.CloseElement();
        }
    }
}
