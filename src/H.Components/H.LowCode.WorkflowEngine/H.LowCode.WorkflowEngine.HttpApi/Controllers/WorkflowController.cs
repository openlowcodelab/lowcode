using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace H.LowCode.WorkflowEngine.HttpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        [HttpGet]
        public string Get()
        {
            return "ok";
        }

        [HttpPost]
        [Route("SaveWorkflow")]
        public void SaveWorkflow(string json)
        {
        }
    }
}
