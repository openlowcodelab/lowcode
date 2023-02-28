using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.OtherComponent
{
    internal class CustomHtmlRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentSettingSchema jsonSchema)
        {
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string keys, ComponentSettingSchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            throw new NotImplementedException();
        }
    }
}
