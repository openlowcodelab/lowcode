using H.Ddd.HttpApi;
using H.LowCode.Admin.Application.Abstraction.AppServices;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Admin.HttpApi.Controllers
{
    public class AdminController : ControllerApiBase
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
