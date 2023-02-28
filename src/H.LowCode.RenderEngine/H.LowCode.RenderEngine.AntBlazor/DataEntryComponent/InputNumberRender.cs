using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class InputNumberRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.Number)
                return true;
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentSettingSchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "");
            builder.AddContent(3, $"{jsonSchema.Title}：");
            builder.CloseElement();

            builder.OpenComponent(0, typeof(InputNumber<int>));
            builder.CloseComponent();
        }
    }
}
