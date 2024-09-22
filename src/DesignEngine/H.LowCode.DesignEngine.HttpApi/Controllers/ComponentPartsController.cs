using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.MetaSchema;
using H.LowCode.Model;
using H.LowCode.PartsMetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class ComponentPartsController : ControllerBase
{
    public ComponentPartsController()
    {
    }

    [HttpGet]
    public async Task<IList<ComponentPartsListModel>> GetListAsync()
    {
        return new List<ComponentPartsListModel>();
    }

    [HttpPost]
    public async Task SaveAsync(ComponentPartsSchema partsSchema)
    {
        await Task.CompletedTask;
    }

    [HttpGet]
    public async Task<ComponentPartsSchema> GetAsync(string partsId)
    {
        return new();
    }

    [HttpGet]
    public async Task DeleteAsync(string partsId)
    {
        await Task.CompletedTask;
    }
}
