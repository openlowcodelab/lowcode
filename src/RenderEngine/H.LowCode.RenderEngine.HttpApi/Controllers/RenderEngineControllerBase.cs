using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public abstract class RenderEngineControllerBase : ControllerBase
{
}
