using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 员工模板管理服务接口
/// </summary>
public interface IAgentTemplateAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<AgentTemplateDto>>> GetListAsync(AgentTemplateQueryDto input);
    Task<BaseOutput<AgentTemplateDto>> GetAsync(Guid id);
    Task<BaseOutput> DeleteAsync(Guid id);
}
