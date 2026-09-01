using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 需求管理服务接口
/// </summary>
public interface IRequirementAppService : IAppService
{
    /// <summary>
    /// 获取项目的需求列表（含关联用例数）
    /// </summary>
    Task<BaseOutput<List<RequirementDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 创建需求
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(RequirementDto dto);

    /// <summary>
    /// 更新需求
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, RequirementDto dto);

    /// <summary>
    /// 删除需求（同时移除用例关联）
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 获取需求关联的用例ID列表
    /// </summary>
    Task<BaseOutput<List<long>>> GetLinkedCaseIdsAsync(long requirementId);

    /// <summary>
    /// 设置需求关联的用例（全量替换）
    /// </summary>
    Task<BaseOutput<bool>> SetCaseLinksAsync(long requirementId, List<long> caseIds);

    /// <summary>
    /// 获取需求追溯视图（关联用例执行情况 + 相关缺陷）
    /// </summary>
    Task<BaseOutput<RequirementTraceDto>> GetTraceAsync(long requirementId);
}
