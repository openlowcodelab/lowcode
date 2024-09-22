using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.PartsMetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class AppPartsController : ControllerBase
{
    public AppPartsController()
    {
    }

    [HttpGet]
    public async Task<IList<AppPartsSchema>> GetListAsync()
    {
        return new List<AppPartsSchema>();
    }
}
