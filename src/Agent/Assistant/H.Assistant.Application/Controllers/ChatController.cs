using H.Assistant.Application.Contracts;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace H.Assistant.Application.Controllers;

/// <summary>
/// 聊天控制器，提供 SSE 流式响应
/// </summary>
[ApiController]
[Route("api/assistant/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatMessageAppService _chatAppService;
    private readonly IChatAppService _sessionAppService;

    public ChatController(IChatMessageAppService chatAppService, IChatAppService sessionAppService)
    {
        _chatAppService = chatAppService;
        _sessionAppService = sessionAppService;
    }

    /// <summary>
    /// 发送消息并获取流式响应（SSE）
    /// </summary>
    [HttpPost("stream")]
    public async Task StreamMessageAsync([FromBody] SendChatMessageInputDto input)
    {
        // 禁用响应缓冲，确保每次 FlushAsync 都立即将数据推送到客户端
        var bufferingFeature = HttpContext.Features.Get<IHttpResponseBodyFeature>();
        bufferingFeature?.DisableBuffering();

        // 设置 SSE 响应头
        Response.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache, no-transform"; // no-transform 防止响应压缩中间件缓冲 SSE 数据
        Response.Headers.Connection = "keep-alive";

        try
        {
            // 如果需要创建新会话，先创建并发送 sessionId 事件
            var sessionId = input.SessionId;
            if (!sessionId.HasValue)
            {
                var agentType = input.AgentType ?? "general";
                var title = input.Message.Length > 30 ? input.Message[..30] + "..." : input.Message;
                sessionId = await _sessionAppService.CreateSessionAsync(title, agentType);
                input.SessionId = sessionId;

                // 发送 session 事件，让前端获取 sessionId（包含 type 字段保持事件格式一致）
                var sessionEvent = $"data: {JsonSerializer.Serialize(new { type = "session", session = sessionId.Value })}\n\n";
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(sessionEvent));
                await Response.Body.FlushAsync();
            }

            await foreach (var chunk in _chatAppService.SendMessageStreamAsync(input))
            {
                // 发送 SSE 格式的数据
                var data = $"data: {chunk}\n\n";
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(data));
                await Response.Body.FlushAsync();
            }

            // 发送结束标记
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes("data: [DONE]\n\n"));
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            // 发送错误信息（使用 JsonSerializer 防止消息中的特殊字符破坏 JSON）
            var errorJson = JsonSerializer.Serialize(new { type = "error", message = ex.Message, isFatal = true });
            var errorData = $"data: {errorJson}\n\n";
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(errorData));
            await Response.Body.FlushAsync();
        }
    }
}
