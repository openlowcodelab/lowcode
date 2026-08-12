using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace H.Assistant.Application;

public class ChatAppService : ApplicationService, IChatAppService
{
    private readonly IRepository<ChatEntity, Guid> _sessionRepository;
    private readonly IRepository<ChatMessageEntity, Guid> _messageRepository;
    private readonly IMapper _objectMapper;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public ChatAppService(
        IRepository<ChatEntity, Guid> sessionRepository,
        IRepository<ChatMessageEntity, Guid> messageRepository,
        IMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _objectMapper = objectMapper;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<Guid> CreateSessionAsync(string title, string agentType = "general")
    {
        var session = new ChatEntity
        {
            Title = title,
            AgentType = agentType,
            LastMessageTime = DateTime.UtcNow,
            MessageCount = 0
        };

        await _sessionRepository.InsertAsync(session);

        return session.Id;
    }

    public async Task<ChatDto?> GetSessionAsync(Guid sessionId)
    {
        var session = await _sessionRepository.FindAsync(sessionId);
        if (session == null)
            return null;

        return _objectMapper.Map<ChatEntity, ChatDto>(session);
    }

    public async Task<List<ChatDto>> GetSessionsAsync(string? filter = null)
    {
        var queryable = await _sessionRepository.GetQueryableAsync();

        var query = queryable.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(s => s.Title.Contains(filter));
        }

        query = query.OrderByDescending(s => s.LastMessageTime);

        var sessions = await _asyncExecuter.ToListAsync(query);

        return sessions
            .Select(s => _objectMapper.Map<ChatEntity, ChatDto>(s))
            .ToList();
    }

    public async Task AddMessageAsync(Guid sessionId, ChatMessageDto message)
    {
        var session = await _sessionRepository.FindAsync(sessionId);

        if (session == null)
        {
            throw new InvalidOperationException($"会话 {sessionId} 不存在，无法添加消息");
        }

        var messageEntity = new ChatMessageEntity
        {
            SessionId = sessionId,
            Role = message.Role,
            Content = message.Content,
            ToolName = message.ToolName,
            ToolResult = message.ToolResult
        };

        await _messageRepository.InsertAsync(messageEntity);

        session.LastMessageTime = DateTime.UtcNow;
        session.MessageCount++;

        await _sessionRepository.UpdateAsync(session);
    }

    public async Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
    {
        var queryable = await _messageRepository.GetQueryableAsync();

        var messages = await _asyncExecuter.ToListAsync(
            queryable
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreationTime)
        );

        return messages
            .Select(m => _objectMapper.Map<ChatMessageEntity, ChatMessageDto>(m))
            .ToList();
    }

    public async Task DeleteSessionAsync(Guid sessionId)
    {
        var messageQueryable = await _messageRepository.GetQueryableAsync();
        var messages = await _asyncExecuter.ToListAsync(
            messageQueryable.Where(m => m.SessionId == sessionId)
        );

        foreach (var message in messages)
        {
            await _messageRepository.DeleteAsync(message);
        }

        await _sessionRepository.DeleteAsync(sessionId);
    }
}
