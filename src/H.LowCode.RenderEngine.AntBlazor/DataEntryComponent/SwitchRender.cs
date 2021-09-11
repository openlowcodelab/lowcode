using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.DataEntryComponent
{
    internal class SwitchRender : ComponentRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            if (jsonSchema.Type != JSchemaType.String)
                return false;

            return false;
        }

        public override void Render(RenderTreeBuilder builder, string key, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            builder.OpenComponent(0, typeof(Input<string>));
            builder.CloseComponent();
        }
    }
}
