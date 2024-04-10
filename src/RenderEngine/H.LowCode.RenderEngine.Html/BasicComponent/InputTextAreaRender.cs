using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputTextAreaRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (string.Equals(jsonSchema.Format, "textarea", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            builder.AddAttribute(2, "style", "vertical-align: top;");
            if (jsonSchema.IsRequired)
            {
                builder.AddMarkupContent(3, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(4, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "textarea");
            builder.AddAttribute(1, "class", "field-value");
            builder.AddAttribute(2, "style", "height:50px;");

            if (jsonSchema.IsRequired)
                builder.AddAttribute(3, "required", "required");

            builder.CloseElement();
        }
    }
}
