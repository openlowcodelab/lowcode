using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

public interface IKnowledgeDocumentAppService : IApplicationService
{
    // Node (tree structure) operations
    Task<List<KnowledgeNodeDto>> GetTreeAsync();
    Task<KnowledgeNodeDto> CreateNodeAsync(CreateKnowledgeNodeDto input);
    Task<KnowledgeNodeDto> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input);
    Task DeleteNodeAsync(Guid nodeId);

    // Document content operations
    Task<KnowledgeDocumentDto?> GetDocumentAsync(Guid nodeId);
    Task<KnowledgeDocumentDto> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input);
}
