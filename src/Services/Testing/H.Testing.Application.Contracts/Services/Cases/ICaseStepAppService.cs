using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例步骤服务接口
/// </summary>
public interface ICaseStepAppService : IAppService
{
    /// <summary>
    /// 获取用例的步骤列表（按 Order 排序）
    /// </summary>
    Task<BaseOutput<List<CaseStepDto>>> GetByCaseIdAsync(long caseId);

    /// <summary>
    /// 批量加载多个用例的步骤并按用例分组（避免逐个用例查询）
    /// </summary>
    Task<BaseOutput<Dictionary<long, List<CaseStepDto>>>> GetByCaseIdsAsync(IEnumerable<long> caseIds);

    /// <summary>
    /// 批量保存用例步骤（新增场景）
    /// </summary>
    Task<BaseOutput> SaveAsync(long caseId, List<CaseStepDto> steps);

    /// <summary>
    /// 同步用例步骤：按 Id 主键更新已有、插入新增、删除已移除（保证步骤 Id 稳定）
    /// </summary>
    Task<BaseOutput> SyncAsync(long caseId, List<CaseStepDto> steps);

    /// <summary>
    /// 删除用例下的全部步骤
    /// </summary>
    Task<BaseOutput> DeleteByCaseIdAsync(long caseId);
}
