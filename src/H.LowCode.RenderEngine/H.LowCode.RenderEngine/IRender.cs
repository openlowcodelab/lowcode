using Microsoft.AspNetCore.Components;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine
{
    public interface IRender
    {
        RenderFragment Render(JSchema jsonSchema, PageRenderType pageRenderType);
    }
}
