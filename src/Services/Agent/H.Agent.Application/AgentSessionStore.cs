using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H.Agent.Application.Contracts;

namespace H.Agent.Application;

/// <summary>
/// Agent 会话存储（内存实现）
/// </summary>
public class AgentSessionStore
{
    private readonly ConcurrentDictionary<Guid, ChatSessionDto> _sessions = new();
    private readonly ConcurrentDictionary<Guid, List<ChatMessageDto>> _messages = new();
    
    public Task<Guid> CreateSessionAsync(string title)
    {
        var sessionId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        _sessions[sessionId] = new ChatSessionDto
        {
            Id = sessionId,
            Title = title,
            CreationTime = now,
            LastMessageTime = now,
            MessageCount = 0
        };
        
        _messages[sessionId] = new List<ChatMessageDto>();
        
        return Task.FromResult(sessionId);
    }
    
    public Task<ChatSessionDto?> GetSessionAsync(Guid sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }
    
    public Task<List<ChatSessionDto>> GetSessionsAsync(string? filter = null)
    {
        var query = _sessions.Values.AsEnumerable();
        
        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(s => s.Title.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }
        
        var result = query
            .OrderByDescending(s => s.LastMessageTime)
            .ToList();
        
        return Task.FromResult(result);
    }
    
    public Task AddMessageAsync(Guid sessionId, ChatMessageDto message)
    {
        if (!_messages.ContainsKey(sessionId))
        {
            _messages[sessionId] = new List<ChatMessageDto>();
        }
        
        _messages[sessionId].Add(message);
        
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastMessageTime = DateTime.UtcNow;
            session.MessageCount = _messages[sessionId].Count;
        }
        
        return Task.CompletedTask;
    }
    
    public Task<List<ChatMessageDto>> GetMessagesAsync(Guid sessionId)
    {
        if (_messages.TryGetValue(sessionId, out var messages))
        {
            return Task.FromResult(messages.OrderBy(m => m.CreationTime).ToList());
        }
        
        return Task.FromResult(new List<ChatMessageDto>());
    }
    
    public Task DeleteSessionAsync(Guid sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
        _messages.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }
}
