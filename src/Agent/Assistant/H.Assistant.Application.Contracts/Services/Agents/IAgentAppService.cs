using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface IAgentAppService : IAppService
{
    Task<BaseOutput<AgentDto>> GetAsync(Guid id);
    Task<BaseOutput<PagedResultDto<AgentDto>>> GetListAsync(AgentQueryDto input);
    Task<BaseOutput<AgentDto>> CreateAsync(CreateAgentDto input);
    Task<BaseOutput<AgentDto>> UpdateAsync(Guid id, UpdateAgentDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<BaseOutput<List<AgentDto>>> GetEnabledAgentsAsync();
    Task<BaseOutput> AddSkillAsync(Guid agentId, Guid skillId);
    Task<BaseOutput> RemoveSkillAsync(Guid agentId, Guid skillId);
    Task<BaseOutput<List<SkillDto>>> GetAgentSkillsAsync(Guid agentId);
}
