using H.Extensions.System;
using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace H.LowCode.DesignEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class DesignEngineController : ControllerBase
{
    private IDesignEngineAppService _designAppService;
    private IEnumerable<SiteOption> _sites;

    public DesignEngineController(IDesignEngineAppService designEngineAppService, IOptions<List<SiteOption>> siteOptions)
    {
        _designAppService = designEngineAppService;
        _sites = siteOptions.Value;
    }

    [HttpGet]
    public async Task<IList<AppListModel>> GetAppsAsync()
    {
        var appSchemas = await _designAppService.GetAppsAsync();
        return appSchemas.Select(x => new AppListModel
        {
            Id = x.Id,
            SiteUrl = _sites.FirstOrDefault(t => t.AppId.Equals(x.Id, StringComparison.OrdinalIgnoreCase))?.SiteUrl,
            Name = x.Name,
            Description = x.Description
        }).ToList();
    }

    [HttpPost]
    public async Task SaveAppAsync(AppSchema appSchema)
    {
        await _designAppService.SaveAppAsync(appSchema);
    }

    [HttpGet]
    public async Task<AppSchema> GetAppAsync(string appId)
    {
        return await _designAppService.GetAppAsync(appId);
    }

    [HttpGet]
    public async Task<MenuSchema> GetMenuAsync(string appId, string menuId)
    {
        return await _designAppService.GetMenuAsync(appId, menuId);
    }

    [HttpGet]
    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _designAppService.GetMenusAsync(appId);
    }

    [HttpPost]
    public async Task SaveMenuAsync(MenuSchema menuSchema)
    {
        await _designAppService.SaveMenuAsync(menuSchema);
    }

    [HttpGet]
    public async Task DeleteMenuAsync(string appId, string menuId)
    {
        await _designAppService.DeleteMenuAsync(appId, menuId);
    }

    [HttpGet]
    public async Task<List<PageListModel>> GetPagesAsync(string appId)
    {
        return await _designAppService.GetPagesAsync(appId);
    }

    [HttpGet]
    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _designAppService.GetPageAsync(appId, pageId);
    }

    [HttpPost]
    public async Task SavePageAsync(PageSchema pageSchema)
    {
        await _designAppService.SavePageAsync(pageSchema);
    }

    [HttpGet]
    public async Task DeletePageAsync(string appId, string pageId)
    {
        await _designAppService.DeletePageAsync(appId, pageId);
    }
}
