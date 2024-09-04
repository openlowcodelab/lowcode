using H.LowCode.MetaSchema;
using H.LowCode.Workbench.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.Workbench.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public class WorkbenchController : ControllerBase
{
    private IWorkbenchAppService _workbenchAppService;

    public WorkbenchController(IWorkbenchAppService workbenchAppService)
    {
        _workbenchAppService = workbenchAppService;
    }
}
