using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.MetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.ComponentRenders
{
    internal class InputRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            jsonSchema.ExtensionData.TryGetValue("widget", out var widget);
            if (widget == null && (string.IsNullOrEmpty(jsonSchema.Format) ||
                string.Equals(jsonSchema.Format, "textarea", StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            if (string.Equals(jsonSchema.Format, "textarea", StringComparison.OrdinalIgnoreCase))
                builder.OpenComponent(0, typeof(TextArea));
            else
                builder.OpenComponent(0, typeof(Input<string>));

            builder.AddAttribute(1, "Placeholder", jsonSchema.Title);
            if (jsonSchema.ExtensionData.TryGetValue("required", out var required))
                builder.AddAttribute(2, "required", "required");
            builder.CloseComponent();
        }
    }
}
