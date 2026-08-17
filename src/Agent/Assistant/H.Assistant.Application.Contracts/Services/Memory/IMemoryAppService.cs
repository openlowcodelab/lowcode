using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface IMemoryAppService : IAppService
{
    Task<BaseOutput<List<KnowledgeNodeDto>>> GetTreeAsync();
    Task<BaseOutput<KnowledgeNodeDto>> CreateNodeAsync(CreateKnowledgeNodeDto input);
    Task<BaseOutput<KnowledgeNodeDto>> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input);
    Task<BaseOutput> DeleteNodeAsync(Guid nodeId);
    Task<BaseOutput<KnowledgeDocumentDto?>> GetDocumentAsync(Guid nodeId);
    Task<BaseOutput<KnowledgeDocumentDto>> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input);
    /// <summary>
    /// 便捷方法：自动查找/创建 Category 目录，在其下创建记忆文档节点
    /// </summary>
    Task<BaseOutput<KnowledgeNodeDto>> CreateMemoryEntryAsync(CreateMemoryEntryDto input);
}
