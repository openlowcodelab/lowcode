using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace H.LowCode.DesignEngine.HttpApi;

public class DesignerController : DesignEngineControllerBase
{
    private IAppApplicationService _appApplicationService;
    private IMenuAppService _menuAppService;
    private IPageAppService _pageAppService;

    public DesignerController(IAppApplicationService designEngineAppService,
        IMenuAppService menuAppService,
        IPageAppService pageAppService)
    {
        _appApplicationService = designEngineAppService;
        _menuAppService = menuAppService;
        _pageAppService = pageAppService;
    }

    [HttpGet]
    public async Task<IList<AppListModel>> GetAppsAsync()
    {
        return await _appApplicationService.GetAppsAsync();
    }

    [HttpPost]
    public async Task SaveAppAsync(AppSchema appSchema)
    {
        await _appApplicationService.SaveAsync(appSchema);
    }

    [HttpGet]
    public async Task<AppSchema> GetAppAsync(string appId)
    {
        return await _appApplicationService.GetByIdAsync(appId);
    }

    [HttpGet]
    public async Task<MenuSchema> GetMenuAsync(string appId, string menuId)
    {
        return await _menuAppService.GetByIdAsync(appId, menuId);
    }

    [HttpGet]
    public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
    {
        return await _menuAppService.GetListAsync(appId);
    }

    [HttpPost]
    public async Task SaveMenuAsync(MenuSchema menuSchema)
    {
        await _menuAppService.SaveAsync(menuSchema);
    }

    [HttpGet]
    public async Task DeleteMenuAsync(string appId, string menuId)
    {
        await _menuAppService.DeleteAsync(appId, menuId);
    }

    [HttpGet]
    public async Task<List<PageListModel>> GetPagesAsync(string appId)
    {
        return await _pageAppService.GetListAsync(appId);
    }

    [HttpGet]
    public async Task<PageSchema> GetPageAsync(string appId, string pageId)
    {
        return await _pageAppService.GetByIdAsync(appId, pageId);
    }

    [HttpPost]
    public async Task SavePageAsync(PageSchema pageSchema)
    {
        await _pageAppService.SaveAsync(pageSchema);
    }

    [HttpGet]
    public async Task DeletePageAsync(string appId, string pageId)
    {
        await _pageAppService.DeleteAsync(appId, pageId);
    }
}
