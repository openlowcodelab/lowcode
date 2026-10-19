using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 课时资源应用服务：视频课程、课件、教学设计、学习任务单、课后练习的登记与直链补录。
/// 资源清单随教材详情一起返回（GET /api/app/textbook/{id}/detail），这里只提供写入端点。
/// ABP 约定控制器生成的端点（前端由 IAppService 动态代理调用）：
///  - POST   /api/app/teaching-resource       新增资源
///  - PUT    /api/app/teaching-resource/{id}  修改资源（补录直链走这里）
///  - DELETE /api/app/teaching-resource/{id}  删除资源
/// </summary>
public interface ITeachingResourceAppService : IAppService
{
    /// <summary>新增课时资源</summary>
    Task<BaseOutput<TeachingResourceDto>> CreateAsync(CreateTeachingResourceDto input);

    /// <summary>修改课时资源</summary>
    Task<BaseOutput<TeachingResourceDto>> UpdateAsync(Guid id, UpdateTeachingResourceDto input);

    /// <summary>删除课时资源</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
