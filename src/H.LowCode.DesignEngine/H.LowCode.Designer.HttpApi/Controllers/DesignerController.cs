using H.Ddd.HttpApi;
using H.LowCode.Designer.Application.Abstraction.AppService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Schema;

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
        [Route("SaveMetadata")]
        public void SaveMetadata([FromForm] string jsonSchema)
        {
            _designerAppService.SaveMetadata(jsonSchema);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonSchema"></param>
        [HttpPost]
        [Route("SaveMetadata2")]
        public void SaveMetadata2([FromBody] JSchema jsonSchema)
        {
            _designerAppService.SaveMetadata2(jsonSchema);
        }
    }
}
