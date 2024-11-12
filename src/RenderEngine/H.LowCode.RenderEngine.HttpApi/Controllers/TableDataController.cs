using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.HttpApi;

public class TableDataController : RenderEngineControllerBase
{
    public async Task<IEnumerable<JsonObject>> GetListAsync()
    {
        await Task.Delay(1);
        return null;
    }
}
