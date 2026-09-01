using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试数据集服务接口（数据驱动测试）
/// </summary>
public interface ITestDatasetAppService : IAppService
{
    /// <summary>
    /// 获取项目的数据集列表（不含数据行明细）
    /// </summary>
    Task<BaseOutput<List<TestDatasetDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 获取数据集详情（含数据行）
    /// </summary>
    Task<BaseOutput<TestDatasetDto?>> GetByIdAsync(long id);

    /// <summary>
    /// 创建数据集
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(TestDatasetDto dto);

    /// <summary>
    /// 更新数据集
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, TestDatasetDto dto);

    /// <summary>
    /// 删除数据集
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);
}
