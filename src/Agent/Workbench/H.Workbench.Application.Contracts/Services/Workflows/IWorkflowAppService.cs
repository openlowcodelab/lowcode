using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Workbench.Application.Contracts;

/// <summary>
/// 员工工作流管理服务接口
/// </summary>
public interface IWorkflowAppService : IAppService
{
    Task<BaseOutput<PagedResultDto<WorkflowDto>>> GetListAsync(WorkflowQueryDto input);
    Task<BaseOutput<WorkflowDto>> GetAsync(Guid id);
    Task<BaseOutput<WorkflowDto>> CreateAsync(CreateWorkflowDto input);
    Task<BaseOutput<WorkflowDto>> UpdateAsync(Guid id, UpdateWorkflowDto input);
    Task<BaseOutput> DeleteAsync(Guid id);
}
