using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.File.Application.Contracts;

/// <summary>
/// 文件项目应用服务接口（项目管理，对应 MinIO Bucket）
/// </summary>
public interface IFileProjectAppService : IAppService
{
    /// <summary>获取所有项目列表</summary>
    Task<BaseOutput<List<FileProjectDto>>> GetListAsync();

    /// <summary>获取单个项目</summary>
    Task<BaseOutput<FileProjectDto>> GetAsync(Guid id);

    /// <summary>创建项目（同时创建 MinIO Bucket）</summary>
    Task<BaseOutput<FileProjectDto>> CreateAsync(CreateFileProjectDto input);

    /// <summary>更新项目信息</summary>
    Task<BaseOutput<FileProjectDto>> UpdateAsync(Guid id, UpdateFileProjectDto input);

    /// <summary>删除项目（同时删除 MinIO Bucket 及其中所有文件）</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
