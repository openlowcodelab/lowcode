using AutoMapper;
using H.Agent.Application.Contracts;
using H.Agent.EntityFrameworkCore;

namespace H.Agent.Application;

public class LLMConfigProfile : Profile
{
    public LLMConfigProfile()
    {
        CreateMap<LLMConfigEntity, LLMConfigDto>();
        CreateMap<CreateLLMConfigDto, LLMConfigEntity>();
        
        // Chat session & message mapping
        CreateMap<AgentChatSessionEntity, ChatSessionDto>();
        CreateMap<AgentChatMessageEntity, ChatMessageDto>();

        // Scheduled task mapping
        CreateMap<AgentScheduledTaskEntity, ScheduledTaskDto>();
        CreateMap<AgentTaskExecutionLogEntity, TaskExecutionLogDto>();
    }
}
