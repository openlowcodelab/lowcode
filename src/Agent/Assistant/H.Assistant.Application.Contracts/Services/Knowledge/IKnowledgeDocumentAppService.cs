using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

public interface IKnowledgeDocumentAppService : IAppService
{
    // Node (tree structure) operations
    Task<List<KnowledgeNodeDto>> GetTreeAsync(Guid knowledgeBaseId);
    Task<KnowledgeNodeDto> CreateNodeAsync(CreateKnowledgeNodeDto input);
    Task<KnowledgeNodeDto> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input);
    Task DeleteNodeAsync(Guid nodeId);

    // Document content operations
    Task<KnowledgeDocumentDto?> GetDocumentAsync(Guid nodeId);
    Task<KnowledgeDocumentDto> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input);
}
