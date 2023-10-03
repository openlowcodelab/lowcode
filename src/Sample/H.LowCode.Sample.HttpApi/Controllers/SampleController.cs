using H.Ddd.HttpApi;
using H.LowCode.Sample.Application.Abstraction.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Sample.HttpApi.Controllers
{
    public class SampleController : ControllerApiBase
    {
        private ISampleAppService _sampleAppService;

        public SampleController(ISampleAppService sampleAppService)
        {
            _sampleAppService = sampleAppService;
        }

        [HttpGet]
        public string Get()
        {
            return "ok";
        }

        [HttpPost]
        public void SaveMetadata([FromForm] string pageSchema)
        {
            _sampleAppService.SaveMetadata(pageSchema);
        }
    }
}
