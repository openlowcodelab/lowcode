using H.Abstractions;
using Volo.Abp.Application.Services;
using H.Assistant.Application.Contracts;
using H.Assistant.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace H.Assistant.Application;

public class ChatMessageAppService : ApplicationService, IChatMessageAppService
{
    private readonly IChatAppService _sessionAppService;
    private readonly AgentFactory _agentFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ChatMessageAppService> _logger;
    
    public ChatMessageAppService(
        IChatAppService sessionAppService, 
        AgentFactory agentFactory,
        IServiceProvider serviceProvider,
        ILogger<ChatMessageAppService> logger)
    {
        _sessionAppService = sessionAppService;
        _agentFactory = agentFactory;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    public async Task<ChatResponseDto> SendMessageAsync(SendChatMessageInputDto input)
    {
        var agentType = input.AgentType ?? "general";
        Guid sessionId;
        
        // 如果是新会话（SessionId 为 null），创建新会话
        if (!input.SessionId.HasValue)
        {
            var title = input.Message.Length > 30 ? input.Message[..30] + "..." : input.Message;
            sessionId = await _sessionAppService.CreateSessionAsync(title, agentType);
        }
        else
        {
            // 使用已存在的会话
            sessionId = input.SessionId.Value;
        }
        
        // 添加用户消息
        var userMessage = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = "user",
            Content = input.Message,
            CreationTime = DateTime.UtcNow
        };
        
        // 获取历史消息（在添加当前用户消息之前，避免 ProcessMessageAsync 中重复添加）
        var history = await _sessionAppService.GetMessagesAsync(sessionId);
        var conversationHistory = history.Select(m => $"{m.Role}: {m.Content}").ToList();
        
        await _sessionAppService.AddMessageAsync(sessionId, userMessage);
        
        // 获取 Assistant 实例
        IAgentInstance? agent = await _agentFactory.CreateAgentAsync(agentType, input.ModelConfigId);
        
        if (agent == null)
        {
            _logger.LogError("无法创建 Agent 实例，agentType={AgentType}, modelConfigId={ModelConfigId}", agentType, input.ModelConfigId);
            throw new InvalidOperationException(
                $"无法创建 Assistant 实例: {agentType}。" +
                $"请检查: 1) LLM 配置是否存在且已启用; 2) API Key 是否正确配置; 3) Agent 定义是否存在。");
        }
        
        // 处理消息
        var response = await agent.ProcessMessageAsync(input.Message, conversationHistory);
        
        // 添加 AI 响应消息
        var aiMessage = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = "assistant",
            Content = response,
            CreationTime = DateTime.UtcNow
        };
        await _sessionAppService.AddMessageAsync(sessionId, aiMessage);
        
        // Trigger memory extraction in background
        _ = Task.Run(() => TryExtractMemoryAsync(sessionId));
        
        return new ChatResponseDto
        {
            SessionId = sessionId,
            MessageId = aiMessage.Id,
            Response = response,
            IsStreaming = false,
            ToolCalls = agent.GetAvailableTools()
        };
    }
    
    public async IAsyncEnumerable<string> SendMessageStreamAsync(SendChatMessageInputDto input)
    {
        var agentType = input.AgentType ?? "general";
        Guid sessionId;
        
        // 如果是新会话（SessionId 为 null），创建新会话
        if (!input.SessionId.HasValue)
        {
            var title = input.Message.Length > 30 ? input.Message[..30] + "..." : input.Message;
            sessionId = await _sessionAppService.CreateSessionAsync(title, agentType);
        }
        else
        {
            // 使用已存在的会话
            sessionId = input.SessionId.Value;
        }
        
        // 添加用户消息
        var userMessage = new ChatMessageDto
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Role = "user",
            Content = input.Message,
            CreationTime = DateTime.UtcNow
        };
        
        // 获取历史消息
        var history = await _sessionAppService.GetMessagesAsync(sessionId);
        var conversationHistory = history.Select(m => $"{m.Role}: {m.Content}").ToList();
        
        await _sessionAppService.AddMessageAsync(sessionId, userMessage);
        
        // 获取 Assistant 实例
        IAgentInstance? agent;
        if (input.ModelConfigId.HasValue)
        {
            agent = await _agentFactory.CreateAgentAsync(agentType, input.ModelConfigId.Value);
        }
        else
        {
            agent = await _agentFactory.CreateAgentAsync(agentType, input.ProviderName);
        }
        
        if (agent == null)
        {
            _logger.LogError("无法创建 Agent 实例(流式)，agentType={AgentType}, providerName={ProviderName}, modelConfigId={ModelConfigId}", 
                agentType, input.ProviderName, input.ModelConfigId);
            throw new InvalidOperationException(
                $"无法创建 Assistant 实例: {agentType}。" +
                $"请检查: 1) LLM 配置是否存在且已启用; 2) API Key 是否正确配置; 3) Agent 定义是否存在。");
        }
        
        // 如果 Assistant 支持流式响应，使用流式处理
        if (agent is IStreamingAgent streamingAgent)
        {
            var fullResponse = string.Empty;
            var reactSteps = new List<object>();
            
            // 逐块接收 ReAct 事件并推送给客户端
            await foreach (var chunk in streamingAgent.ProcessMessageStreamAsync(input.Message, conversationHistory))
            {
                // 解析 JSON 事件，提取内容用于保存
                try
                {
                    using var doc = JsonDocument.Parse(chunk);
                    var root = doc.RootElement;
                    
                    if (root.TryGetProperty("type", out var typeProp))
                    {
                        var eventType = typeProp.GetString();
                        
                        // 累积最终回答内容（来自 answer 事件）
                        if (eventType == "answer" && root.TryGetProperty("content", out var contentProp))
                        {
                            var content = contentProp.GetString() ?? "";
                            fullResponse += content;
                        }
                        
                        // 收集 ReAct 步骤数据（thinking、tool_call、tool_result）
                        if (eventType == "thinking" || eventType == "tool_call" || eventType == "tool_result")
                        {
                            var iteration = root.TryGetProperty("iteration", out var iterProp) ? iterProp.GetInt32() : 0;
                            var stepData = new Dictionary<string, object?>
                            {
                                ["type"] = eventType,
                                ["iteration"] = iteration
                            };
                            
                            if (eventType == "thinking" && root.TryGetProperty("content", out var tc))
                            {
                                stepData["content"] = tc.GetString();
                            }
                            else if (eventType == "tool_call")
                            {
                                if (root.TryGetProperty("toolName", out var tn)) stepData["toolName"] = tn.GetString();
                                if (root.TryGetProperty("arguments", out var args)) stepData["arguments"] = args.GetString();
                            }
                            else if (eventType == "tool_result")
                            {
                                if (root.TryGetProperty("toolName", out var trn)) stepData["toolName"] = trn.GetString();
                                if (root.TryGetProperty("result", out var res)) stepData["result"] = res.GetString();
                                if (root.TryGetProperty("isError", out var ie)) stepData["isError"] = ie.GetBoolean();
                            }
                            
                            reactSteps.Add(stepData);
                        }
                    }
                }
                catch
                {
                    // 非 JSON 格式（向后兼容旧格式），直接累积
                    fullResponse += chunk;
                }
                
                yield return chunk;
            }
            
            // 保存完整的 AI 响应消息（将 ReAct 步骤嵌入 Content 字段）
            if (!string.IsNullOrWhiteSpace(fullResponse))
            {
                var contentToSave = fullResponse;
                
                // 如果有 ReAct 步骤，将其与最终回答一起打包为 JSON 格式
                if (reactSteps.Count > 0)
                {
                    var enrichedContent = new Dictionary<string, object?>
                    {
                        ["answer"] = fullResponse,
                        ["reactSteps"] = reactSteps
                    };
                    contentToSave = JsonSerializer.Serialize(enrichedContent, new JsonSerializerOptions 
                    { 
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
                    });
                }
                
                var aiMessage = new ChatMessageDto
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    Role = "assistant",
                    Content = contentToSave,
                    CreationTime = DateTime.UtcNow
                };
                await _sessionAppService.AddMessageAsync(sessionId, aiMessage);
                
                // Trigger memory extraction in background
                _ = Task.Run(() => TryExtractMemoryAsync(sessionId));
            }
        }
        else
        {
            // 降级到同步处理
            var response = await agent.ProcessMessageAsync(input.Message, conversationHistory);
            
            var aiMessage = new ChatMessageDto
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Role = "assistant",
                Content = response,
                CreationTime = DateTime.UtcNow
            };
            await _sessionAppService.AddMessageAsync(sessionId, aiMessage);
            
            // Trigger memory extraction in background
            _ = Task.Run(() => TryExtractMemoryAsync(sessionId));
            
            // 一次性返回完整响应
            yield return response;
        }
    }
    
    public async Task<PagedResultDto<ChatDto>> GetSessionsAsync(SessionQueryDto input)
    {
        var sessions = await _sessionAppService.GetSessionsAsync(input.Filter);
        
        var totalCount = sessions.Count;
        var pagedSessions = sessions
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();
        
        return new PagedResultDto<ChatDto>(totalCount, pagedSessions);
    }
    
    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
    {
        return await _sessionAppService.GetMessagesAsync(sessionId);
    }
    
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        await _sessionAppService.DeleteSessionAsync(sessionId);
    }
    
    public async Task<List<AgentConfigDto>> GetAvailableAgentsAsync()
    {
        var agents = await _agentFactory.GetAvailableAgentsAsync();
        return agents.Select(a => new AgentConfigDto
        {
            AgentType = a.AgentType,
            DisplayName = a.DisplayName,
            Description = a.Description,
            Capabilities = a.Capabilities
        }).ToList();
    }

    /// <summary>
    /// 从对话中异步提取记忆（后台执行，不影响主流程）
    /// </summary>
    private async Task TryExtractMemoryAsync(Guid sessionId)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var llmFactory = scope.ServiceProvider.GetRequiredService<LLMProviderFactory>();
            var memoryService = scope.ServiceProvider.GetRequiredService<IMemoryAppService>();
            var chatService = scope.ServiceProvider.GetRequiredService<IChatAppService>();

            // Get recent messages from this session
            var messages = await chatService.GetMessagesAsync(sessionId);
            if (messages.Count < 2) return; // Need at least 1 user + 1 assistant message

            // Only take the last 10 messages for extraction
            var recentMessages = messages.TakeLast(10).ToList();

            // Build conversation text for extraction
            var conversationText = string.Join("\n", recentMessages.Select(m => $"{m.Role}: {m.Content}"));

            // Get default LLM provider
            var provider = await llmFactory.GetDefaultProviderAsync();
            if (provider == null)
            {
                _logger.LogWarning("无法提取记忆：没有可用的默认 LLM Provider");
                return;
            }

            // Build extraction prompt
            var systemPrompt = @"你是一个信息提取助手。请从以下对话中提取值得记住的关键信息，包括：用户偏好、项目信息、技术决策、重要事实等。
以 JSON 数组格式返回，每项包含 title、content、category 字段。category 可选值：用户偏好、项目信息、技术决策、重要事实、其他。
如果没有值得提取的信息，返回空数组 []。
只返回 JSON，不要其他内容。";

            var request = new LLMRequest
            {
                Messages = new List<Message>
                {
                    new Message { Role = "system", Content = systemPrompt },
                    new Message { Role = "user", Content = $"请从以下对话中提取关键信息：\n\n{conversationText}" }
                },
                Temperature = 0.3f,
                MaxTokens = 1000
            };

            var response = await provider.ChatAsync(request);
            var responseText = response.Content.Trim();

            // Extract JSON from response (handle potential markdown code blocks)
            if (responseText.StartsWith("```"))
            {
                var jsonStart = responseText.IndexOf('\n') + 1;
                var jsonEnd = responseText.LastIndexOf("```");
                if (jsonEnd > jsonStart)
                {
                    responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart).Trim();
                }
            }

            // Parse and save memories
            var memories = JsonSerializer.Deserialize<List<MemoryExtractionResult>>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (memories != null && memories.Count > 0)
            {
                foreach (var mem in memories)
                {
                    if (string.IsNullOrWhiteSpace(mem.Title) || string.IsNullOrWhiteSpace(mem.Content))
                        continue;

                    await memoryService.CreateMemoryEntryAsync(new CreateMemoryEntryDto
                    {
                        Title = mem.Title.Trim(),
                        Content = mem.Content.Trim(),
                        Category = mem.Category?.Trim() ?? "其他"
                    });
                }

                _logger.LogInformation("从会话 {SessionId} 中提取了 {Count} 条记忆", sessionId, memories.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "从会话 {SessionId} 提取记忆失败", sessionId);
        }
    }

    private class MemoryExtractionResult
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Category { get; set; }
    }
}
