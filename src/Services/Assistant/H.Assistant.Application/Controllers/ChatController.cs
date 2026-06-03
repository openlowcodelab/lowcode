using System.Text;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using H.Assistant.Application.Contracts;

namespace H.Assistant.Application.Controllers;

/// <summary>
/// 聊天控制器，提供 SSE 流式响应
/// </summary>
[ApiController]
[Route("api/assistant/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatMessageAppService _chatAppService;

    public ChatController(IChatMessageAppService chatAppService)
    {
        _chatAppService = chatAppService;
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
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        try
        {
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
            // 发送错误信息
            var errorData = $"data: {{\"error\": \"{ex.Message}\"}}\n\n";
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(errorData));
            await Response.Body.FlushAsync();
        }
    }
}
