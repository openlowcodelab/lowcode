using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// Agent 聊天服务实现
/// </summary>
public class AgentChatAppService : ApplicationService, IAgentChatAppService
{
    private readonly IAgentSessionAppService _sessionAppService;
    private readonly AgentFactory _agentFactory;
    
    public AgentChatAppService(IAgentSessionAppService sessionAppService, AgentFactory agentFactory)
    {
        _sessionAppService = sessionAppService;
        _agentFactory = agentFactory;
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
        
        // 获取 Agent 实例
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
            throw new InvalidOperationException($"无法创建 Agent 实例: {agentType}");
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
        
        return new ChatResponseDto
        {
            SessionId = sessionId,
            MessageId = aiMessage.Id,
            Response = response,
            IsStreaming = false,
            ToolCalls = agent.GetAvailableTools()
        };
    }
    
    public async Task<PagedResultDto<ChatSessionDto>> GetSessionsAsync(SessionQueryDto input)
    {
        var sessions = await _sessionAppService.GetSessionsAsync(input.Filter);
        
        var totalCount = sessions.Count;
        var pagedSessions = sessions
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();
        
        return new PagedResultDto<ChatSessionDto>(totalCount, pagedSessions);
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
        var agents = _agentFactory.GetAvailableAgents();
        return agents.Select(a => new AgentConfigDto
        {
            AgentType = a.AgentType,
            DisplayName = a.DisplayName,
            Description = a.Description,
            Capabilities = a.Capabilities
        }).ToList();
    }
}
