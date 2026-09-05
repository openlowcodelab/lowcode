using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 缺陷管理服务接口
/// </summary>
public interface IDefectAppService : IAppService
{
    /// <summary>
    /// 获取项目的缺陷列表（可按状态/严重程度过滤）
    /// </summary>
    Task<BaseOutput<List<DefectDto>>> GetByProjectIdAsync(long projectId, int? status = null, int? severity = null);

    /// <summary>
    /// 获取用例关联的缺陷列表
    /// </summary>
    Task<BaseOutput<List<DefectDto>>> GetByCaseIdAsync(long caseId);

    /// <summary>
    /// 创建缺陷
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(DefectDto dto);

    /// <summary>
    /// 更新缺陷
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, DefectDto dto);

    /// <summary>
    /// 删除缺陷
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);
}
