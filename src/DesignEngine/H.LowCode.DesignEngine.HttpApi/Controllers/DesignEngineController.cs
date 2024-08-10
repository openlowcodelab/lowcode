using H.Extensions.System;
using H.LowCode.Admin.DTO;
using H.LowCode.DesignEngine.Application.Abstraction;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DesignEngineController : ControllerBase
    {
        private IDesignEngineAppService _designAppService;

        public DesignEngineController(IDesignEngineAppService designEngineAppService)
        {
            _designAppService = designEngineAppService;
        }

        [HttpGet]
        public async Task<IList<AppSchema>> GetAppsAsync()
        {
            return await _designAppService.GetAppsAsync();
        }

        [HttpPost]
        public async Task SaveAppAsync(AppSchema app)
        {
            await _designAppService.SaveAppAsync(app);
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
        public async Task<List<PageListModel>> GetPagesAsync(string appId)
        {
            return await _designAppService.GetPagesAsync(appId);
        }

        [HttpGet]
        public async Task<string> GetPageAsync(string appId, string pageId)
        {
            return await _designAppService.GetPageAsync(appId, pageId);
        }

        [HttpPost]
        public async Task SavePageAsync(PageSchema pageSchema)
        {
            await _designAppService.SavePageAsync(pageSchema);
        }
    }
}
