using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置项管理接口
/// </summary>
public interface ISettingValueAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<SettingValueDto>>> GetListAsync(SettingValueQueryDto input);
    Task<BaseOutput<SettingValueDto>> GetAsync(Guid id);
    Task<BaseOutput<SettingValueDto>> CreateAsync(CreateUpdateSettingValueDto input);
    Task<BaseOutput<SettingValueDto>> UpdateAsync(Guid id, CreateUpdateSettingValueDto input);
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>获取全部配置定义下拉项（供配置项编辑时选择）</summary>
    Task<BaseOutput<List<SettingDefinitionLookupDto>>> GetDefinitionLookupAsync();
}
