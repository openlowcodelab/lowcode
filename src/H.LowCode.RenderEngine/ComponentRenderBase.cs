using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine
{
    public abstract class ComponentRenderBase
    {
        public abstract void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func);

        public abstract bool CanRender(JSchema jsonSchema);
    }
}
