using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class CheckboxRender : ElementRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.String)
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

        public override void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            if (jsonSchema.ExtensionData.TryGetValue("widget", out var widget))
            {
                if (string.Equals(widget.ToString(), "checkboxs", StringComparison.OrdinalIgnoreCase))
                {
                    builder.OpenComponent(0, typeof(CheckboxGroup));

                    jsonSchema.ExtensionData.TryGetValue("enumNames", out JToken enumNames);
                    var names = enumNames.ToObject<string[]>();
                    for (int i = 0; i < jsonSchema.Enum.Count; i++)
                    {
                        builder.OpenComponent(i * 3 + 5, typeof(Checkbox));
                        builder.AddAttribute(i * 3 + 6, "Label", names[i]);
                        builder.AddContent(i * 3 + 7, jsonSchema.Enum[i].ToObject<string>());
                        builder.CloseComponent();
                    }

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
