using H.Ddd.HttpApi;
using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Abstraction.AppServices;
using H.LowCode.MetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi.Controllers
{
    public class DesignEngineController : ControllerApiBase
    {
        private IDesignEngineAppService _designEngineAppService;

        public DesignEngineController(IDesignEngineAppService designEngineAppService)
        {
            _designEngineAppService = designEngineAppService;
        }

        [HttpGet]
        public string GetPageSchema(string appId, string pageId)
        {
            return _designEngineAppService.GetPageSchema(appId, pageId);
        }

        [HttpPost]
        public void SavePageSchema(PageSchema pageSchema)
        {
            _designEngineAppService.SavePageSchema(pageSchema.AppId, pageSchema.PageId, pageSchema.ToJson());
        }
    }
}
