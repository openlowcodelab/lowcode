using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 知识库管理应用服务（知识库的增删改查）
/// </summary>
public interface IKnowledgeBaseAppService : IAppService
{
    Task<BaseOutput<List<KnowledgeBaseDto>>> GetListAsync();
    Task<BaseOutput<KnowledgeBaseDto>> GetAsync(Guid id);
    Task<BaseOutput<KnowledgeBaseDto>> CreateAsync(CreateKnowledgeBaseDto input);
    Task<BaseOutput<KnowledgeBaseDto>> UpdateAsync(Guid id, UpdateKnowledgeBaseDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}
