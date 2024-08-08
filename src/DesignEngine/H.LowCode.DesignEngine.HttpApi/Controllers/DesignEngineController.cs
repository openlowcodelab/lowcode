using H.Extensions.System;
using H.LowCode.Admin.DTO;
using H.LowCode.DesignEngine.Application.Abstraction.AppServices;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi.Controllers
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
        public IList<AppSchema> GetApps()
        {
            return _designAppService.GetApps();
        }

        [HttpPost]
        public void SaveApp(AppSchema app)
        {
            _designAppService.SaveApp(app);
        }

        [HttpGet]
        public IList<MenuSchema> GetMenus(string appId)
        {
            return _designAppService.GetMenus(appId);
        }

        [HttpPost]
        public void SaveMenu(MenuSchema menuSchema)
        {
            _designAppService.SaveMenu(menuSchema);
        }

        [HttpGet]
        public List<PageListModel> GetPages(string appId)
        {
            return _designAppService.GetPages(appId);
        }

        [HttpGet]
        public string GetPage(string appId, string pageId)
        {
            return _designAppService.GetPage(appId, pageId);
        }

        [HttpPost]
        public void SavePage(PageSchema pageSchema)
        {
            _designAppService.SavePage(pageSchema);
        }
    }
}
