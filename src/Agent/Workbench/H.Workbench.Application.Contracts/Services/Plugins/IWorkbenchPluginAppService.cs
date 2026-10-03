using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 插件（能力包）管理服务接口
/// </summary>
public interface IWorkbenchPluginAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<WorkbenchPluginDto>>> GetListAsync(WorkbenchPluginQueryDto input);
    Task<BaseOutput<WorkbenchPluginDto>> GetAsync(Guid id);

    /// <summary>
    /// 按员工绑定关系取插件（运行时授予技能用）。不对外开 HTTP 入口，理由同连接器。
    /// </summary>
    Task<BaseOutput<List<WorkbenchPluginDto>>> ListByIdsAsync(List<Guid> ids);

    Task<BaseOutput<WorkbenchPluginDto>> CreateAsync(CreateWorkbenchPluginDto input);
    Task<BaseOutput<WorkbenchPluginDto>> UpdateAsync(Guid id, UpdateWorkbenchPluginDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
    Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled);
}
