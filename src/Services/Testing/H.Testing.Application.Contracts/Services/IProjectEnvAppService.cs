using H.Abp.Application.Contracts;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目环境服务接口
/// </summary>
public interface IProjectEnvAppService : IAppService
{
    /// <summary>
    /// 获取所有项目环境（所有项目）
    /// </summary>
    Task<List<ProjectEnvDto>> GetAllAsync();

    /// <summary>
    /// 根据项目ID获取环境列表
    /// </summary>
    Task<List<ProjectEnvDto>> GetByProjectIdAsync(long projectId);

    /// <summary>
    /// 根据ID获取项目环境
    /// </summary>
    Task<ProjectEnvDto?> GetByIdAsync(long id);

    /// <summary>
    /// 创建新的项目环境
    /// </summary>
    Task<long> CreateAsync(ProjectEnvDto environment);

    /// <summary>
    /// 更新项目环境
    /// </summary>
    Task<bool> UpdateAsync(long id, ProjectEnvDto environment);

    /// <summary>
    /// 删除项目环境
    /// </summary>
    Task<bool> DeleteAsync(long id);

    /// <summary>
    /// 根据类型获取环境
    /// </summary>
    Task<List<ProjectEnvDto>> GetByTypeAsync(EnvironmentType type);
}
