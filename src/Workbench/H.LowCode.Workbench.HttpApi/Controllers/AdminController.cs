using H.LowCode.Workbench.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Workbench.HttpApi;

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
