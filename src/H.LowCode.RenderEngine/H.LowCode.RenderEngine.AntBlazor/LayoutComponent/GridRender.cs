using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using H.LowCode.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.LayoutComponent
{
    internal class GridRender : ComponentRenderBase
    {
        public override bool CanRender(ComponentPropertySchema jsonSchema)
        {
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string keys, ComponentPropertySchema jsonSchema, Func<PageSchema, RenderFragment> func)
        {
            throw new NotImplementedException();
        }
    }
}
