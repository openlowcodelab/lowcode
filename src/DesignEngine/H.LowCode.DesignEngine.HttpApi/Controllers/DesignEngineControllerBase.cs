using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.PartsMetaSchema;
using Microsoft.AspNetCore.Mvc;

namespace H.LowCode.DesignEngine.HttpApi;

[ApiController]
[Route("api/[controller]/[action]")]
public abstract class DesignEngineControllerBase : ControllerBase
{
}
