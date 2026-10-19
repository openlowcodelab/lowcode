using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 学生应用服务：学生档案维护，答题与趣味练习都从这里选学生。
/// ABP 约定控制器生成的端点：
///  - GET    /api/app/student        分页列表（含答题数、平均得分率、练习局数）
///  - GET    /api/app/student/{id}   单个学生
///  - POST   /api/app/student        新增
///  - PUT    /api/app/student/{id}   修改
///  - DELETE /api/app/student/{id}   删除档案（历史答卷与练习记录保留）
/// </summary>
public interface IStudentAppService : IAppService
{
    /// <summary>分页查询学生</summary>
    Task<BaseOutput<PagedResultDto<StudentDto>>> GetListAsync(StudentQueryDto input);

    /// <summary>获取学生</summary>
    Task<BaseOutput<StudentDto>> GetAsync(Guid id);

    /// <summary>新增学生</summary>
    Task<BaseOutput<StudentDto>> CreateAsync(CreateStudentDto input);

    /// <summary>更新学生</summary>
    Task<BaseOutput<StudentDto>> UpdateAsync(Guid id, UpdateStudentDto input);

    /// <summary>删除学生档案</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
