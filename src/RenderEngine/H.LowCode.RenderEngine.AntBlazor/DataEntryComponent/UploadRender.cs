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
    internal class UploadRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            if (jsonSchema.ComponentValueType != ComponentValueType.String)
                return false;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, ComponentSettingSchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            builder.OpenComponent(0, typeof(Input<string>));
            builder.CloseComponent();
        }
    }
}
