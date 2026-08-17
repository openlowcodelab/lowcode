using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目环境服务接口
/// </summary>
public interface IProjectEnvAppService : IAppService
{
    /// <summary>
    /// 获取所有项目环境（所有项目）
    /// </summary>
    Task<BaseOutput<List<ProjectEnvDto>>> GetAllAsync();

    /// <summary>
    /// 根据项目ID获取环境列表
    /// </summary>
    Task<BaseOutput<List<ProjectEnvDto>>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 根据ID获取项目环境
    /// </summary>
    Task<BaseOutput<ProjectEnvDto?>> GetByIdAsync(long id);

    /// <summary>
    /// 创建新的项目环境
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(ProjectEnvDto environment);

    /// <summary>
    /// 更新项目环境
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, ProjectEnvDto environment);

    /// <summary>
    /// 删除项目环境
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 根据类型获取环境
    /// </summary>
    Task<BaseOutput<List<ProjectEnvDto>>> GetByTypeAsync(EnvironmentType type);
}
