using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using H.Agent.Application.Contracts;

namespace H.Agent.Application.Services;

/// <summary>
/// Agent 聊天服务实现
/// </summary>
public class AgentChatAppService : ApplicationService, IAgentChatAppService
{
    private readonly AgentSessionStore _sessionStore;
    private readonly AgentFactory _agentFactory;
    
    public AgentChatAppService(AgentSessionStore sessionStore, AgentFactory agentFactory)
    {
        _sessionStore = sessionStore;
        _agentFactory = agentFactory;
    }
    
    public async Task<ChatResponseDto> SendMessageAsync(SendChatMessageInputDto input)
    {
        var sessionId = input.SessionId ?? Guid.NewGuid();
        var agentType = input.AgentType ?? "general";
        
        // 如果是新会话，创建会话
        var existingSession = await _sessionStore.GetSessionAsync(sessionId);
        if (existingSession == null)
        {
            var title = input.Message.Length > 30 ? input.Message[..30] + "..." : input.Message;
            sessionId = await _sessionStore.CreateSessionAsync(title);
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
        await _sessionStore.AddMessageAsync(sessionId, userMessage);
        
        // 获取 Agent 实例
        var agent = await _agentFactory.CreateAgentAsync(agentType);
        if (agent == null)
        {
            throw new InvalidOperationException($"无法创建 Agent 实例: {agentType}");
        }
        
        // 获取历史消息
        var history = await _sessionStore.GetMessagesAsync(sessionId);
        var conversationHistory = history.Select(m => $"{m.Role}: {m.Content}").ToList();
        
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
        await _sessionStore.AddMessageAsync(sessionId, aiMessage);
        
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
        var sessions = await _sessionStore.GetSessionsAsync(input.Filter);
        
        var totalCount = sessions.Count;
        var pagedSessions = sessions
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();
        
        return new PagedResultDto<ChatSessionDto>(totalCount, pagedSessions);
    }
    
    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
    {
        return await _sessionStore.GetMessagesAsync(sessionId);
    }
    
    public async Task DeleteSessionAsync(Guid sessionId)
    {
        await _sessionStore.DeleteSessionAsync(sessionId);
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
