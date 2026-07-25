using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置项管理接口
/// </summary>
public interface ISettingValueAppService : IApplicationService
{
    Task<PagedResultDto<SettingValueDto>> GetListAsync(SettingValueQueryDto input);
    Task<SettingValueDto> GetAsync(Guid id);
    Task<SettingValueDto> CreateAsync(CreateUpdateSettingValueDto input);
    Task<SettingValueDto> UpdateAsync(Guid id, CreateUpdateSettingValueDto input);
    Task DeleteAsync(Guid id);

    /// <summary>获取全部配置定义下拉项（供配置项编辑时选择）</summary>
    Task<List<SettingDefinitionLookupDto>> GetDefinitionLookupAsync();
}
