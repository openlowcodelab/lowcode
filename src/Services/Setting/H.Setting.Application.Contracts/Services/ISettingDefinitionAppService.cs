using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Setting.Application.Contracts;

/// <summary>
/// 配置定义管理接口
/// </summary>
public interface ISettingDefinitionAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<SettingDefinitionDto>>> GetListAsync(SettingDefinitionQueryDto input);
    Task<BaseOutput<SettingDefinitionDto>> GetAsync(Guid id);
    Task<BaseOutput<SettingDefinitionDto>> CreateAsync(CreateUpdateSettingDefinitionDto input);
    Task<BaseOutput<SettingDefinitionDto>> UpdateAsync(Guid id, CreateUpdateSettingDefinitionDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}
