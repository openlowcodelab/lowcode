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
    internal class CascaderRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenComponent(0, typeof(Cascader));
            builder.CloseComponent();
        }
    }
}
