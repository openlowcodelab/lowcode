using H.Ddd.HttpApi;
using H.LowCode.DesignEngine.Application.Abstraction.AppServices;
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
        public string Get()
        {
            return "ok";
        }

        [HttpPost]
        public void SavePageSchema([FromForm] string appId, [FromForm] string pageId, [FromForm] string pageSchema)
        {
            _designEngineAppService.SavePageSchema(appId, pageId, pageSchema);
        }

        [HttpGet]
        public string GetPageSchema(string appId, string pageId)
        {
            return _designEngineAppService.GetPageSchema(appId, pageId);
        }
    }
}
