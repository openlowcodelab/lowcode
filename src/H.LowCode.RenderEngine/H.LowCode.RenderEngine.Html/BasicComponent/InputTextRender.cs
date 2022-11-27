using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Schema;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputTextRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            jsonSchema.ExtensionData.TryGetValue("widget", out var widget);
            if (string.IsNullOrEmpty(jsonSchema.Format) && widget == null)
                return true;
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            if (jsonSchema.ExtensionData.TryGetValue("required", out var required))
            {
                builder.AddMarkupContent(2, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "input");
            builder.AddAttribute(1, "type", "text");
            builder.AddAttribute(2, "class", "field-value");

            if (jsonSchema.IsRequired)
                builder.AddAttribute(3, "required", "required");

            builder.CloseElement();

            //无需调用委托，input元素不存在子元素
        }
    }
}
