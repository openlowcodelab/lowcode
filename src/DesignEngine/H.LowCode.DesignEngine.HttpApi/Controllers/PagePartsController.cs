using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.MetaSchema;
using H.LowCode.Model;
using H.LowCode.PartsMetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class PagePartsController : ControllerBase
{
    public PagePartsController()
    {
    }

    [HttpGet]
    public async Task<IList<PagePartsSchema>> GetListAsync()
    {
        return new List<PagePartsSchema>();
    }

    [HttpPost]
    public async Task SaveAsync(PagePartsSchema partsSchema)
    {
        await Task.CompletedTask;
    }

    [HttpGet]
    public async Task<PagePartsSchema> GetAsync(string partsId)
    {
        return new();
    }

    [HttpGet]
    public async Task DeleteAsync(string partsId)
    {
        await Task.CompletedTask;
    }
}
