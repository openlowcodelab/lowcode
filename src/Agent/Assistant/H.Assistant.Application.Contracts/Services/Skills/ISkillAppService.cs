using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Assistant.Application.Contracts;

public interface ISkillAppService : IAppService
{
    Task<BaseOutput<SkillDto>> GetAsync(Guid id);
    Task<BaseOutput<PagedResultDto<SkillDto>>> GetListAsync(SkillDefinitionQueryDto input);
    Task<BaseOutput<SkillDto>> CreateAsync(CreateSkillDefinitionDto input);
    Task<BaseOutput<SkillDto>> UpdateAsync(Guid id, UpdateSkillDefinitionDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<BaseOutput<List<SkillDto>>> GetEnabledSkillsAsync();
    Task<BaseOutput<List<SkillDto>>> GetSkillsByTypeAsync(string skillType);
    Task<BaseOutput> IncrementUsageCountAsync(Guid id);
}
