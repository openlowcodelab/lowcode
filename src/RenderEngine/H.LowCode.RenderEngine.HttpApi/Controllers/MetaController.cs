using H.Extensions.System;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.RenderEngine.HttpApi;

public class MetaController : RenderEngineControllerBase
{
    private IMetaAppService _metaAppService;

    public MetaController(IMetaAppService metaAppService)
    {
        _metaAppService = metaAppService;
    }

    [HttpGet]
    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _metaAppService.GetMenusAsync(appId);
    }

    [HttpGet]
    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _metaAppService.GetPageAsync(appId, pageId);
    }
}
