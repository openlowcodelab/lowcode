using AutoMapper;
using H.AI.Application.Contracts;
using H.AI.EntityFrameworkCore;

namespace H.AI.Application;

public class AIMapperProfile : Profile
{
    public AIMapperProfile()
    {
        CreateMap<LLMEntity, LLMDto>();
        CreateMap<CreateLLMDto, LLMEntity>();

        // Knowledge base mapping
        CreateMap<KnowledgeBaseEntity, KnowledgeBaseDto>();
        CreateMap<CreateKnowledgeBaseDto, KnowledgeBaseEntity>();

        // Knowledge node (tree structure) mapping
        CreateMap<KnowledgeNodeEntity, KnowledgeNodeDto>();
        CreateMap<CreateKnowledgeNodeDto, KnowledgeNodeEntity>();

        // Knowledge document (content) mapping
        CreateMap<KnowledgeDocumentEntity, KnowledgeDocumentDto>();
    }
}
