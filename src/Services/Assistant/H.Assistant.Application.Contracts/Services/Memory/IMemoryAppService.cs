using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

public interface IMemoryAppService : IApplicationService
{
    Task<List<KnowledgeNodeDto>> GetTreeAsync();
    Task<KnowledgeNodeDto> CreateNodeAsync(CreateKnowledgeNodeDto input);
    Task<KnowledgeNodeDto> UpdateNodeAsync(Guid nodeId, UpdateKnowledgeNodeDto input);
    Task DeleteNodeAsync(Guid nodeId);
    Task<KnowledgeDocumentDto?> GetDocumentAsync(Guid nodeId);
    Task<KnowledgeDocumentDto> SaveDocumentAsync(Guid nodeId, SaveKnowledgeDocumentDto input);
    /// <summary>
    /// 便捷方法：自动查找/创建 Category 目录，在其下创建记忆文档节点
    /// </summary>
    Task<KnowledgeNodeDto> CreateMemoryEntryAsync(CreateMemoryEntryDto input);
}
