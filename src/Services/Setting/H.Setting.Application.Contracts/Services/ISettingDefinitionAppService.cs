using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置定义管理接口
/// </summary>
public interface ISettingDefinitionAppService : IApplicationService
{
    Task<PagedResultDto<SettingDefinitionDto>> GetListAsync(SettingDefinitionQueryDto input);
    Task<SettingDefinitionDto> GetAsync(Guid id);
    Task<SettingDefinitionDto> CreateAsync(CreateUpdateSettingDefinitionDto input);
    Task<SettingDefinitionDto> UpdateAsync(Guid id, CreateUpdateSettingDefinitionDto input);
    Task DeleteAsync(Guid id);
}
