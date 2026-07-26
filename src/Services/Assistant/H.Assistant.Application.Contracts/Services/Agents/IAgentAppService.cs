using H.Abstractions;

namespace H.Assistant.Application.Contracts;

public interface IAgentAppService : IAppService
{
    Task<AgentDto> GetAsync(Guid id);
    Task<PagedResultDto<AgentDto>> GetListAsync(AgentQueryDto input);
    Task<AgentDto> CreateAsync(CreateAgentDto input);
    Task<AgentDto> UpdateAsync(Guid id, UpdateAgentDto input);
    Task DeleteAsync(Guid id);
    Task ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<List<AgentDto>> GetEnabledAgentsAsync();
    Task AddSkillAsync(Guid agentId, Guid skillId);
    Task RemoveSkillAsync(Guid agentId, Guid skillId);
    Task<List<SkillDto>> GetAgentSkillsAsync(Guid agentId);
}
