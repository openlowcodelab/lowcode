using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine;

namespace H.LowCode.Parts.DefaultComponents.Render
{
    internal class CheckboxRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkbox", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (string.Equals(widget.ToString(), "checkboxs", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkboxs", StringComparison.OrdinalIgnoreCase))
                {
                    builder.OpenComponent(0, typeof(CheckboxGroup<int>));

                    //jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
                    //var names = enumNames.ToObject<string[]>();
                    //for (int i = 0; i < jsonSchema.Enum.Count; i++)
                    //{
                    //    builder.OpenComponent(i * 3 + 5, typeof(Checkbox));
                    //    builder.AddAttribute(i * 3 + 6, "Label", names[i]);
                    //    builder.AddContent(i * 3 + 7, jsonSchema.Enum[i].ToObject<string>());
                    //    builder.CloseComponent();
                    //}

                    builder.CloseElement();
                }
                else
                {
                    builder.OpenComponent(0, typeof(Checkbox));
                    builder.CloseComponent();
                }
            }
        }
    }
}
