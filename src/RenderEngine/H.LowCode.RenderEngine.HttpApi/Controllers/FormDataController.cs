using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.HttpApi;

public class FormDataController : RenderEngineControllerBase
{
    public async Task<JsonObject> GetAsync()
    {
        await Task.Delay(1);
        return null;
    }
}
