using H.Extensions.System;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Application.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.RenderEngine.HttpApi
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RenderEngineController : ControllerBase
    {
        private IRenderEngineAppService _renderAppService;

        public RenderEngineController(IRenderEngineAppService renderEngineAppService)
        {
            _renderAppService = renderEngineAppService;
        }

        [HttpGet]
        public async Task<IList<MenuSchema>> GetMenusAsync(string appId)
        {
            return await _renderAppService.GetMenusAsync(appId);
        }

        [HttpGet]
        public async Task<string> GetPageAsync(string appId, string pageId)
        {
            return await _renderAppService.GetPageAsync(appId, pageId);
        }
    }
}
