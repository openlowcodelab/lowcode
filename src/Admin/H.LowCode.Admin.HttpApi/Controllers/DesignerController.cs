using H.Ddd.HttpApi;
using H.LowCode.Admin.Application.Abstraction.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Admin.HttpApi.Controllers
{
    public class DesignerController : ControllerApiBase
    {
        private IDesignerAppService _designerAppService;

        public DesignerController(IDesignerAppService designerAppService)
        {
            _designerAppService = designerAppService;
        }

        [HttpGet]
        public string Get()
        {
            return "ok";
        }

        [HttpPost]
        public void SaveMetadata([FromForm] string pageSchema)
        {
            _designerAppService.SaveMetadata(pageSchema);
        }
    }
}
