using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试用例服务接口
/// </summary>
public interface ICaseAppService : IAppService
{
    /// <summary>
    /// 获取所有测试用例（所有项目）
    /// </summary>
    Task<BaseOutput<List<CaseDto>>> GetAllAsync();

    /// <summary>
    /// 根据项目 ID 获取测试用例
    /// </summary>
    Task<BaseOutput<List<CaseDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 根据 ID 获取测试用例
    /// </summary>
    Task<BaseOutput<CaseDto?>> GetByIdAsync(long id);

    /// <summary>
    /// 创建新的测试用例
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(CaseDto projectCase);

    /// <summary>
    /// 更新测试用例
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, CaseDto projectCase);

    /// <summary>
    /// 删除测试用例
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 获取激活状态的测试用例
    /// </summary>
    Task<BaseOutput<List<CaseDto>>> GetActiveProjectCasesAsync();

    /// <summary>
    /// 根据级别获取测试用例
    /// </summary>
    Task<BaseOutput<List<CaseDto>>> GetByLevelAsync(CaseLevel level);

    /// <summary>
    /// 搜索测试用例
    /// </summary>
    Task<BaseOutput<List<CaseDto>>> SearchAsync(string keyword);

    /// <summary>
    /// 复制测试用例
    /// </summary>
    Task<BaseOutput<long>> CopyAsync(long id);
}
