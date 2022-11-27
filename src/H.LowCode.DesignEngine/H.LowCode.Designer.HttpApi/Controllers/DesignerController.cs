using H.Ddd.HttpApi;
using H.LowCode.Designer.Application.Abstraction.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Designer.HttpApi.Controllers
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
