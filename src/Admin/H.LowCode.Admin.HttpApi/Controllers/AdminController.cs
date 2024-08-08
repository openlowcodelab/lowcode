using H.LowCode.Admin.Application.Abstraction.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Admin.HttpApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class AdminController : ControllerBase
    {
        private IAdminAppService _adminAppService;

        public AdminController(IAdminAppService adminAppService)
        {
            _adminAppService = adminAppService;
        }

        [HttpGet]
        public string Get()
        {
            return "ok";
        }
    }
}
