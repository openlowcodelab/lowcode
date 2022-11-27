using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Schema;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputCheckboxRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.Boolean)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkbox", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            if (jsonSchema.IsRequired)
            {
                builder.AddMarkupContent(2, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "checkbox");

            if (jsonSchema.IsRequired)
                builder.AddAttribute(3, "required", "required");

            builder.CloseElement();
        }
    }
}
