using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.AntBlazor.ComponentRenders
{
    internal class RedioRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "radio", StringComparison.OrdinalIgnoreCase))
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

            builder.OpenComponent(0, typeof(RadioGroup<string>));

            //jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
            //var names = enumNames.ToObject<string[]>();
            //for (int i = 0; i < jsonSchema.Enum.Count; i++)
            //{
            //    builder.OpenComponent(i * 3 + 5, typeof(Radio<string>));
            //    builder.AddAttribute(i * 3 + 6, "Value", jsonSchema.Enum[i].ToObject<string>());
            //    builder.AddContent(i * 3 + 7, names[i]);
            //    builder.CloseComponent();
            //}

            builder.CloseComponent();
        }
    }
}
