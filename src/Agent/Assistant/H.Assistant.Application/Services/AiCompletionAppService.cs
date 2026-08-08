using H.Assistant.Application.Contracts;
using H.Assistant.Core;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application;

/// <summary>
/// AI 基础能力服务
/// 基于 Assistant 默认模型配置，向其它应用提供统一的文本生成能力
/// </summary>
public class AiCompletionAppService : ApplicationService, IAiCompletionAppService
{
    private readonly LLMProviderFactory _providerFactory;

    public AiCompletionAppService(LLMProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    public async Task<AiCompletionResultDto> CompleteAsync(AiCompletionInputDto input)
    {
        if (string.IsNullOrWhiteSpace(input.UserMessage))
        {
            throw new UserFriendlyException("AI 输入内容不能为空");
        }

        var provider = await _providerFactory.GetDefaultProviderAsync();
        if (provider == null)
        {
            throw new UserFriendlyException("未配置可用的默认 AI 模型，请先在智能助手应用中配置并启用默认模型");
        }

        var request = new LLMRequest
        {
            Temperature = input.Temperature,
            MaxTokens = input.MaxTokens,
            Messages = []
        };

        if (!string.IsNullOrWhiteSpace(input.SystemPrompt))
        {
            request.Messages.Add(new Message { Role = "system", Content = input.SystemPrompt });
        }

        request.Messages.Add(new Message { Role = "user", Content = input.UserMessage });

        LLMResponse response;
        try
        {
            response = await provider.ChatAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw ConvertHttpError(ex);
        }

        return new AiCompletionResultDto
        {
            Content = response.Content,
            Model = response.Model,
            UsageTokens = response.UsageTokens
        };
    }

    /// <summary>
    /// 将模型服务的 HTTP 错误转为友好提示（401 通常为 API Key 错误）
    /// </summary>
    private static UserFriendlyException ConvertHttpError(HttpRequestException ex)
    {
        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized || ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            return new UserFriendlyException("AI 模型认证失败（API Key 无效或已过期），请在智能助手应用的模型管理中检查并更新 API Key");
        }

        return new UserFriendlyException($"AI 模型调用失败：{ex.Message}");
    }
}
