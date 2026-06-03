using AutoMapper;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;

namespace H.Assistant.Application;

public class AssistantMapperProfile : Profile
{
    public AssistantMapperProfile()
    {
        CreateMap<LLMEntity, LLMDto>();
        CreateMap<CreateLLMDto, LLMEntity>();
        
        // Chat session & message mapping
        CreateMap<ChatEntity, ChatDto>();
        CreateMap<ChatMessageEntity, ChatMessageDto>();

        // Scheduled task mapping
        CreateMap<TaskEntity, TaskDto>();
        CreateMap<TaskLogEntity, TaskLogDto>();
    }
}
