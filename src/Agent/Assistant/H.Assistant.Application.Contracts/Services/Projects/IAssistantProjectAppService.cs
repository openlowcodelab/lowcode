using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 项目管理服务接口
/// </summary>
public interface IAssistantProjectAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<AssistantProjectDto>>> GetListAsync(AssistantProjectQueryDto input);
    Task<BaseOutput<AssistantProjectDto>> GetAsync(Guid id);
    Task<BaseOutput<AssistantProjectDto>> CreateAsync(CreateAssistantProjectDto input);
    Task<BaseOutput<AssistantProjectDto>> UpdateAsync(Guid id, UpdateAssistantProjectDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}
