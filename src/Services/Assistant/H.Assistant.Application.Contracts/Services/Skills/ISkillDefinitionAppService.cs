using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Assistant.Application.Contracts;

/// <summary>
/// 技能定义管理服务接口
/// </summary>
public interface ISkillDefinitionAppService : IApplicationService
{
    Task<SkillDefinitionDto> GetAsync(Guid id);
    Task<PagedResultDto<SkillDefinitionDto>> GetListAsync(SkillDefinitionQueryDto input);
    Task<SkillDefinitionDto> CreateAsync(CreateSkillDefinitionDto input);
    Task<SkillDefinitionDto> UpdateAsync(Guid id, UpdateSkillDefinitionDto input);
    Task DeleteAsync(Guid id);
    Task ToggleEnabledAsync(Guid id, bool isEnabled);
    Task<List<SkillDefinitionDto>> GetEnabledSkillsAsync();
    Task<List<SkillDefinitionDto>> GetSkillsByTypeAsync(string skillType);
    Task IncrementUsageCountAsync(Guid id);
}
