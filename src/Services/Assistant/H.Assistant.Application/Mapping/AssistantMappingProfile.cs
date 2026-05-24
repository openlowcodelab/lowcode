using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;

namespace H.Assistant.Application;

public class AssistantMappingProfile : Profile
{
    public AssistantMappingProfile()
    {
        CreateMap<LLMConfigEntity, LLMConfigDto>();
        CreateMap<CreateLLMConfigDto, LLMConfigEntity>();
        
        // Chat session & message mapping
        CreateMap<ChatSessionEntity, ChatSessionDto>();
        CreateMap<ChatMessageEntity, ChatMessageDto>();

        // Scheduled task mapping
        CreateMap<ScheduledTaskEntity, ScheduledTaskDto>();
        CreateMap<TaskExecutionLogEntity, TaskExecutionLogDto>();
    }
}
