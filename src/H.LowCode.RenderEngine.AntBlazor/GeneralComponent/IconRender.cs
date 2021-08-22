﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.RenderEngine.AntBlazor.GeneralComponent
{
    internal class IconRender : ElementRenderBase
    {
        public override bool CanRender(JSchema jsonSchema)
        {
            return false;
        }

        public override void Render(RenderTreeBuilder builder, string keys, JSchema jsonSchema, Func<JSchema, RenderFragment> func)
        {
            throw new NotImplementedException();
        }
    }
}
