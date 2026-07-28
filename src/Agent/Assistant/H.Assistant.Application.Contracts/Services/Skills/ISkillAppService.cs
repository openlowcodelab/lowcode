using H.Abp.Application.Contracts;

namespace H.Assistant.Application.Contracts;

public interface ISkillAppService : IAppService
{
    Task<SkillDto> GetAsync(Guid id);
    Task<PagedResultDto<SkillDto>> GetListAsync(SkillDefinitionQueryDto input);
    Task<SkillDto> CreateAsync(CreateSkillDefinitionDto input);
    Task<SkillDto> UpdateAsync(Guid id, UpdateSkillDefinitionDto input);
    Task DeleteAsync(Guid id);
    Task ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<List<SkillDto>> GetEnabledSkillsAsync();
    Task<List<SkillDto>> GetSkillsByTypeAsync(string skillType);
    Task IncrementUsageCountAsync(Guid id);
}
