using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 知识库管理应用服务（知识库的增删改查）
/// </summary>
public interface IKnowledgeBaseAppService : IAppService
{
    Task<List<KnowledgeBaseDto>> GetListAsync();
    Task<KnowledgeBaseDto> GetAsync(Guid id);
    Task<KnowledgeBaseDto> CreateAsync(CreateKnowledgeBaseDto input);
    Task<KnowledgeBaseDto> UpdateAsync(Guid id, UpdateKnowledgeBaseDto input);
    Task DeleteAsync(Guid id);
}
