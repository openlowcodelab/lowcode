using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// Agent 定义管理服务接口
/// </summary>
public interface IAgentDefinitionAppService : IApplicationService
{
    Task<AgentDefinitionDto> GetAsync(Guid id);
    Task<PagedResultDto<AgentDefinitionDto>> GetListAsync(AgentDefinitionQueryDto input);
    Task<AgentDefinitionDto> CreateAsync(CreateAgentDefinitionDto input);
    Task<AgentDefinitionDto> UpdateAsync(Guid id, UpdateAgentDefinitionDto input);
    Task DeleteAsync(Guid id);
    Task ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<List<AgentDefinitionDto>> GetEnabledAgentsAsync();
    Task AddSkillAsync(Guid agentId, Guid skillId);
    Task RemoveSkillAsync(Guid agentId, Guid skillId);
    Task<List<SkillDefinitionDto>> GetAgentSkillsAsync(Guid agentId);
}
