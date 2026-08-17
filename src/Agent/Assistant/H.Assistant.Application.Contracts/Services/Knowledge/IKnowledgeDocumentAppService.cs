using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface IKnowledgeDocumentAppService : IAppService
{
    // Node (tree structure) operations
    Task<BaseOutput<List<KnowledgeNodeDto>>> GetTreeAsync(Guid knowledgeBaseId);
    Task<BaseOutput<KnowledgeNodeDto>> CreateNodeAsync(CreateKnowledgeNodeDto input);
    Task<BaseOutput<KnowledgeNodeDto>> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input);
    Task<BaseOutput> DeleteNodeAsync(Guid nodeId);

    // Document content operations
    Task<BaseOutput<KnowledgeDocumentDto?>> GetDocumentAsync(Guid nodeId);
    Task<BaseOutput<KnowledgeDocumentDto>> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input);
}
