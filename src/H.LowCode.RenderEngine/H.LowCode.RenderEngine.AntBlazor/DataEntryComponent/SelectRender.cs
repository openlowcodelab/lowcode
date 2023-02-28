using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Metadata;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class SelectRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "select", StringComparison.OrdinalIgnoreCase))
                    return true;
                else if (string.Equals(widget.ToString(), "multiselect", StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string keys, ComponentSettingSchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            //builder.OpenComponent(0, typeof(SimpleSelect));
            //if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            //{
            //    if (string.Equals(widget.ToString(), "multiselect", StringComparison.OrdinalIgnoreCase))
            //        builder.AddAttribute(1, "Mode", "multiple");
            //}
            ////builder.OpenComponent(2, typeof(SelectOptions));

            //jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
            //var names = enumNames.ToObject<string[]>();
            //for (int i = 0; i < jsonSchema.Enum.Count; i++)
            //{
            //    builder.OpenComponent(i * 3 + 5, typeof(SimpleSelectOption));
            //    builder.AddAttribute(i * 3 + 6, "Label", names[i]);
            //    builder.AddAttribute(i * 3 + 7, "Value", jsonSchema.Enum[i].ToObject<string>());
            //    builder.CloseComponent();
            //}

            ////builder.CloseComponent();
            //builder.CloseComponent();
        }
    }
}
