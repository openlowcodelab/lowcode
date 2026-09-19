using H.Workbench.Application.Contracts;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Volo.Abp.Uow;

namespace H.Workbench.Application.Controllers;

/// <summary>
/// 任务控制器，提供 SSE 流式执行
/// </summary>
[ApiController]
[Route("api/workbench/task")]
public class TaskController : ControllerBase
{
    private readonly ITaskAppService _taskAppService;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public TaskController(ITaskAppService taskAppService, IUnitOfWorkManager unitOfWorkManager)
    {
        _taskAppService = taskAppService;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// 流式执行任务（迭代器在控制器内枚举，ABP 拦截器的 UoW 覆盖不到，需显式开启并贯穿整个响应流）
    /// </summary>
    [HttpPost("stream")]
    public async Task ExecuteStreamAsync([FromBody] ExecuteTaskStreamInputDto input)
    {
        // 禁用响应缓冲，确保每次 FlushAsync 都立即将数据推送到客户端
        var bufferingFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
        bufferingFeature?.DisableBuffering();

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-transform"; // no-transform 防止响应压缩中间件缓冲 SSE 数据
        Response.Headers.Connection = "keep-alive";

        using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
        try
        {
            await foreach (var chunk in _taskAppService.ExecuteStreamAsync(input))
            {
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"data: {chunk}\n\n"));
                await Response.Body.FlushAsync();
            }

            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"));
            await Response.Body.FlushAsync();

            await uow.CompleteAsync();
        }
        catch (Exception ex)
        {
            var errorJson = JsonSerializer.Serialize(new { type = "error", message = ex.Message, isFatal = true });
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"data: {errorJson}\n\n"));
            await Response.Body.FlushAsync();
        }
    }
}
