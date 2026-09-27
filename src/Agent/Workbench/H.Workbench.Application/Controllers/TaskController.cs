using H.Util.Base;
using H.Workbench.Application.Contracts;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace H.Workbench.Application.Controllers;

/// <summary>
/// 任务运行端点。阶段F 起执行不再寄生在这条 HTTP 请求里：
/// start 把运行交给后台宿主，stream 只是订阅事件中枢，断开连接不影响执行。
/// </summary>
[ApiController]
[Route("api/workbench/task")]
public class TaskController : ControllerBase
{
    private readonly ITaskAppService _taskAppService;

    public TaskController(ITaskAppService taskAppService)
    {
        _taskAppService = taskAppService;
    }

    /// <summary>
    /// 提交即订阅：启动一次后台运行并把事件流回给当前连接。
    /// 连接断开只会退订，执行照旧跑完。
    /// </summary>
    [HttpPost("stream")]
    public async Task ExecuteStreamAsync([FromBody] ExecuteTaskStreamInputDto input)
    {
        var started = await _taskAppService.StartRunAsync(new StartRunInputDto
        {
            TaskId = input.TaskId,
            Prompt = input.Prompt
        });

        if (!started.Success || started.Data == Guid.Empty)
        {
            OpenEventStream();
            await WriteEventAsync(JsonSerializer.Serialize(
                new { type = "error", message = started.Message ?? "任务无法启动", isFatal = true }));
            return;
        }

        await StreamAsync(started.Data);
    }

    /// <summary>提交一次运行并立刻返回 runId（不建长连接，适合“发完就走”）</summary>
    [HttpPost("start")]
    public Task<BaseOutput<Guid>> StartAsync([FromBody] StartRunInputDto input)
        => _taskAppService.StartRunAsync(input);

    /// <summary>订阅某次运行的事件流：先回放缓冲再接实时。刷新或换设备后重连用这个。</summary>
    [HttpGet("stream/{runId}")]
    public Task StreamByRunAsync(Guid runId) => StreamAsync(runId);

    /// <summary>续跑一次失败的工作流：已成功的步骤不重做，返回新的 runId</summary>
    [HttpPost("resume/{runId}")]
    public Task<BaseOutput<Guid>> ResumeAsync(Guid runId) => _taskAppService.ResumeRunAsync(runId);

    /// <summary>显式取消一次运行（此前“关页面”即取消，现在必须明说）</summary>
    [HttpPost("cancel/{runId}")]
    public Task<BaseOutput> CancelAsync(Guid runId) => _taskAppService.CancelRunAsync(runId);

    /// <summary>运行状态：是否仍在跟踪、当前步数、裁决</summary>
    [HttpGet("status/{runId}")]
    public Task<BaseOutput<RunStatusDto>> StatusAsync(Guid runId) => _taskAppService.GetRunStatusAsync(runId);

    private async Task StreamAsync(Guid runId)
    {
        OpenEventStream();

        // 订阅只读内存事件中枢，不碰数据库，因此不需要 UoW 覆盖整条响应流
        try
        {
            await foreach (var chunk in _taskAppService.SubscribeRunAsync(runId, HttpContext.RequestAborted))
            {
                await WriteEventAsync(chunk);
            }

            await WriteEventAsync("[DONE]");
        }
        catch (Exception ex)
        {
            // 客户端断开时错误也写不回去；执行的收尾由宿主负责，与这条连接无关
            try
            {
                await WriteEventAsync(JsonSerializer.Serialize(
                    new { type = "error", message = ex.Message, isFatal = true }));
            }
            catch (Exception)
            {
                // ignored: connection already aborted
            }
        }
    }

    private void OpenEventStream()
    {
        // 禁用响应缓冲，确保每次 FlushAsync 都立即将数据推送到客户端
        HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-transform"; // no-transform 防止响应压缩中间件缓冲 SSE 数据
        Response.Headers.Connection = "keep-alive";
    }

    private async Task WriteEventAsync(string payload)
    {
        await Response.Body.WriteAsync(Encoding.UTF8.GetBytes($"data: {payload}\n\n"));
        await Response.Body.FlushAsync();
    }
}
