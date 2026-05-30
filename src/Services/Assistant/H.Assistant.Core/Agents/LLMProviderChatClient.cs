using Microsoft.Extensions.AI;

namespace H.Assistant.Core;

/// <summary>
/// 将 ILLMProvider 包装为 IChatClient
/// </summary>
internal class LLMProviderChatClient : IChatClient
{
    private readonly ILLMProvider _provider;

    public LLMProviderChatClient(ILLMProvider provider)
    {
        _provider = provider;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var llmMessages = messageList.Select(m => new Message
        {
            Role = m.Role.ToString().ToLowerInvariant(),
            Content = m.Text
        }).ToList();

        var request = new LLMRequest
        {
            Messages = llmMessages,
            Temperature = options?.Temperature ?? 0.7f,
            MaxTokens = (int)(options?.MaxOutputTokens ?? 2000)
        };

        var response = await _provider.ChatAsync(request, cancellationToken);

        return new ChatResponse(new ChatMessage(ChatRole.Assistant, response.Content))
        {
            ModelId = response.Model,
            Usage = response.UsageTokens > 0 ? new UsageDetails { OutputTokenCount = response.UsageTokens } : null
        };
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();
        var llmMessages = messageList.Select(m => new Message
        {
            Role = m.Role.ToString().ToLowerInvariant(),
            Content = m.Text
        }).ToList();

        var request = new LLMRequest
        {
            Messages = llmMessages,
            Temperature = options?.Temperature ?? 0.7f,
            MaxTokens = (int)(options?.MaxOutputTokens ?? 2000)
        };

        await foreach (var chunk in _provider.ChatStreamAsync(request, cancellationToken))
        {
            yield return new ChatResponseUpdate(ChatRole.Assistant, chunk);
        }
    }

    public object? GetService(Type serviceType, object? key = null) => null;

    public void Dispose()
    {
        // Nothing to dispose
    }
}
