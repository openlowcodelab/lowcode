using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 试卷应用服务：AI 生成试卷、试卷查询与维护。
/// ABP 约定控制器生成的端点（前端由 IAppService 动态代理调用）：
///  - GET    /api/app/exam-paper                     分页列表
///  - GET    /api/app/exam-paper/statistics          按学科统计
///  - GET    /api/app/exam-paper/{id}                试卷基本信息
///  - GET    /api/app/exam-paper/{id}/detail         试卷详情（含题目与答案）
///  - POST   /api/app/exam-paper/generate            生成试卷（内置样卷同步返回，AI 生成落库为"生成中"后交后台任务）
///  - POST   /api/app/exam-paper/{id}/retry-generate AI 生成失败后按原参数重试
///  - PUT    /api/app/exam-paper/{id}                维护基本信息
///  - POST   /api/app/exam-paper/{id}/publish        发布/撤回
///  - DELETE /api/app/exam-paper/{id}                删除试卷及其题目
/// </summary>
public interface IExamPaperAppService : IAppService
{
    /// <summary>分页查询试卷</summary>
    Task<BaseOutput<PagedResultDto<ExamPaperDto>>> GetListAsync(ExamPaperQueryDto input);

    /// <summary>按学科统计试卷数、答题数与平均得分率</summary>
    Task<BaseOutput<List<ExamPaperStatDto>>> GetStatisticsAsync();

    /// <summary>获取试卷基本信息</summary>
    Task<BaseOutput<ExamPaperDto>> GetAsync(Guid id);

    /// <summary>获取试卷详情（含全部题目、参考答案与解析）</summary>
    Task<BaseOutput<ExamPaperDetailDto>> GetDetailAsync(Guid id);

    /// <summary>
    /// 生成试卷：内置样卷同步出题直接返回题目；
    /// AI 生成先落库一张"生成中"的空卷并交给后台任务，返回时题目还没写入，前端轮询列表看进度。
    /// </summary>
    Task<BaseOutput<ExamPaperDetailDto>> GenerateAsync(GeneratePaperInputDto input);

    /// <summary>AI 命题失败后按原入参重新排队</summary>
    Task<BaseOutput<ExamPaperDto>> RetryGenerateAsync(Guid id);

    /// <summary>维护试卷基本信息（标题、时长、说明、状态）</summary>
    Task<BaseOutput<ExamPaperDto>> UpdateAsync(Guid id, UpdateExamPaperDto input);

    /// <summary>发布或撤回试卷，仅已发布试卷可答题</summary>
    Task<BaseOutput<ExamPaperDto>> PublishAsync(Guid id, PublishPaperInputDto input);

    /// <summary>删除试卷（级联删除题目与答卷）</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
