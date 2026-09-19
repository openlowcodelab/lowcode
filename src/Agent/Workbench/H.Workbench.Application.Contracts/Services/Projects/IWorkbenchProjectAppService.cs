using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 项目管理服务接口
/// </summary>
public interface IWorkbenchProjectAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<WorkbenchProjectDto>>> GetListAsync(WorkbenchProjectQueryDto input);
    Task<BaseOutput<WorkbenchProjectDto>> GetAsync(Guid id);
    Task<BaseOutput<List<WorkbenchProjectDto>>> GetByIdsAsync(List<Guid> ids);
    Task<BaseOutput<WorkbenchProjectDto>> CreateAsync(CreateWorkbenchProjectDto input);
    Task<BaseOutput<WorkbenchProjectDto>> UpdateAsync(Guid id, UpdateWorkbenchProjectDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}
