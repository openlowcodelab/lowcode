using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Metadata;

namespace H.LowCode.RenderEngine.Html.BasicComponent
{
    internal class SelectRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if(string.Equals(widget.ToString(), "select", StringComparison.OrdinalIgnoreCase))
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

            builder.OpenElement(0, "select");
            builder.AddAttribute(1, "class", "field-value");

            //jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
            //var names = enumNames.ToObject<string[]>();
            //for (int i = 0; i < jsonSchema.Enum.Count; i++)
            //{
            //    builder.OpenElement(i * 3 + 5, "option");
            //    builder.AddAttribute(i * 3 + 6, "value", jsonSchema.Enum[i]);
            //    builder.AddContent(i * 3 + 7, names[i]);
            //    builder.CloseElement();
            //}

            builder.CloseElement();
        }
    }
}
