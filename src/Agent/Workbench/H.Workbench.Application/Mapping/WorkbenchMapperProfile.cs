using AutoMapper;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;

namespace H.Workbench.Application;

public class WorkbenchMapperProfile : Profile
{
    public WorkbenchMapperProfile()
    {
        CreateMap<LLMEntity, LLMDto>();
        CreateMap<CreateLLMDto, LLMEntity>();

        // Chat session & message mapping
        CreateMap<ChatEntity, ChatDto>();
        CreateMap<ChatMessageEntity, ChatMessageDto>();

        // Scheduled task mapping
        CreateMap<TaskEntity, TaskDto>();
        CreateMap<TaskLogEntity, TaskLogDto>();

        // Knowledge base mapping
        CreateMap<KnowledgeBaseEntity, KnowledgeBaseDto>();
        CreateMap<CreateKnowledgeBaseDto, KnowledgeBaseEntity>();

        // Knowledge node (tree structure) mapping
        CreateMap<KnowledgeNodeEntity, KnowledgeNodeDto>();
        CreateMap<CreateKnowledgeNodeDto, KnowledgeNodeEntity>();

        // Knowledge document (content) mapping
        CreateMap<KnowledgeDocumentEntity, KnowledgeDocumentDto>();

        // Category mapping
        CreateMap<CategoryEntity, CategoryDto>();
    }
}
