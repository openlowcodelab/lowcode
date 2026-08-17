using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 项目服务接口
/// </summary>
public interface IProjectAppService : IAppService
{
    /// <summary>
    /// 获取所有项目
    /// </summary>
    Task<BaseOutput<List<ProjectDto>>> GetAllAsync();

    /// <summary>
    /// 根据ID获取项目
    /// </summary>
    Task<BaseOutput<ProjectDto?>> GetByIdAsync(long id);

    /// <summary>
    /// 创建项目
    /// </summary>
    Task<BaseOutput<long>> CreateAsync(ProjectDto project);

    /// <summary>
    /// 创建项目(兼容旧接口)
    /// </summary>
    Task<BaseOutput<long>> CreateProjectAsync(ProjectDto project);

    /// <summary>
    /// 更新项目
    /// </summary>
    Task<BaseOutput<bool>> UpdateAsync(long id, ProjectDto project);

    /// <summary>
    /// 删除项目及其相关的所有数据
    /// </summary>
    Task<BaseOutput<bool>> DeleteAsync(long id);

    /// <summary>
    /// 获取项目的环境列表
    /// </summary>
    Task<BaseOutput<List<ProjectEnvDto>>> GetProjectEnvironmentsAsync(long projectId);
}
