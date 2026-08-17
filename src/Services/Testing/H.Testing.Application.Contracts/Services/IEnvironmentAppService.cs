using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试环境服务接口
/// </summary>
public interface IEnvironmentAppService : IAppService
{
    /// <summary>
    /// 获取指定项目的所有环境
    /// </summary>
    Task<BaseOutput<List<EnvironmentDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 根据ID获取环境
    /// </summary>
    Task<BaseOutput<EnvironmentDto?>> GetByIdAsync(long projectId, long id);

    /// <summary>
    /// 创建新环境
    /// </summary>
    Task<BaseOutput<EnvironmentDto>> CreateAsync(EnvironmentDto environment);

    /// <summary>
    /// 更新环境
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, EnvironmentDto environment);

    /// <summary>
    /// 删除环境
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long projectId, long id);
}
