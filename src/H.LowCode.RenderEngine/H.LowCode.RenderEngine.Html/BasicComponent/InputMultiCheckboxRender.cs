using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Metadata;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class InputMultiCheckboxRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.Array)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkboxes", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentSettingSchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "field-label");
            if (jsonSchema.IsRequired)
            {
                builder.AddMarkupContent(2, "<span style='color:red;'>*</span>");
            }
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenElement(0, "label");
            builder.AddAttribute(1, "class", "field-value");

            //jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
            //var names = enumNames.ToObject<string[]>();
            //for (int i = 0; i < jsonSchema.Enum.Count; i++)
            //{
            //    builder.OpenElement(i * 3 + 5, "label");
            //    builder.AddMarkupContent(i * 3 + 7, $"<input type='checkbox' value='{jsonSchema.Enum[i]}' >");
            //    builder.AddMarkupContent(i * 3 + 8, $"<span style='margin:0 15px 0 8px;'>{names[i]}</span>");
            //    builder.CloseElement();
            //}

            builder.CloseElement();
        }
    }
}
