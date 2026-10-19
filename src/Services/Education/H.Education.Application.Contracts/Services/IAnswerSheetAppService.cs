using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 答题应用服务：开始答题、保存进度、交卷自动评分。
/// ABP 约定控制器生成的端点：
///  - GET  /api/app/answer-sheet                    分页查询答卷
///  - GET  /api/app/answer-sheet/{id}/result        批改结果（含逐题明细）
///  - POST /api/app/answer-sheet/start              开始答题
///  - POST /api/app/answer-sheet/{id}/save          保存答题进度
///  - POST /api/app/answer-sheet/{id}/submit        交卷并自动评分
///  - DELETE /api/app/answer-sheet/{id}             删除答卷
/// </summary>
public interface IAnswerSheetAppService : IAppService
{
    /// <summary>分页查询答卷记录</summary>
    Task<BaseOutput<PagedResultDto<AnswerSheetDto>>> GetListAsync(AnswerSheetQueryDto input);

    /// <summary>获取批改结果与逐题明细</summary>
    Task<BaseOutput<AnswerResultDto>> GetResultAsync(Guid id);

    /// <summary>开始答题：按试卷创建答卷并返回题目（不含参考答案）</summary>
    Task<BaseOutput<AnswerStartDto>> StartAsync(StartAnswerInputDto input);

    /// <summary>继续答题：按答卷 ID 读取题目与已保存的作答</summary>
    Task<BaseOutput<AnswerStartDto>> GetAnsweringAsync(Guid id);

    /// <summary>保存答题进度，不评分</summary>
    Task<BaseOutput<AnswerSheetDto>> SaveAsync(Guid id, SaveAnswerInputDto input);

    /// <summary>交卷并自动评分：客观题服务端判定，主观题由 AI 批改</summary>
    Task<BaseOutput<AnswerResultDto>> SubmitAsync(Guid id, SubmitAnswerInputDto input);

    /// <summary>删除答卷</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
