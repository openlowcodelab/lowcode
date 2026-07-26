using H.Abp.Application.Contracts;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置定义管理接口
/// </summary>
public interface ISettingDefinitionAppService : IAppService
{
    Task<PagedResultDto<SettingDefinitionDto>> GetListAsync(SettingDefinitionQueryDto input);
    Task<SettingDefinitionDto> GetAsync(Guid id);
    Task<SettingDefinitionDto> CreateAsync(CreateUpdateSettingDefinitionDto input);
    Task<SettingDefinitionDto> UpdateAsync(Guid id, CreateUpdateSettingDefinitionDto input);
    Task DeleteAsync(Guid id);
}
