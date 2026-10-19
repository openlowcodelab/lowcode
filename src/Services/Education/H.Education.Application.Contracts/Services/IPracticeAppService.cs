using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 趣味练习应用服务：口算闯关、单词闯关、诗词填空。
/// 题目由服务端生成并落库（答案不下发到前端），交卷时服务端判定得分与连对。
/// ABP 约定控制器生成的端点：
///  - GET    /api/app/practice            分页查询练习记录
///  - GET    /api/app/practice/games      可玩的练习游戏列表
///  - GET    /api/app/practice/{id}/result 练习结果回顾
///  - POST   /api/app/practice/start      开始一局
///  - POST   /api/app/practice/{id}/submit 交卷结算
///  - DELETE /api/app/practice/{id}       删除记录
/// </summary>
public interface IPracticeAppService : IAppService
{
    /// <summary>获取可玩的练习游戏</summary>
    Task<BaseOutput<List<PracticeGameDto>>> GetGamesAsync();

    /// <summary>分页查询练习记录</summary>
    Task<BaseOutput<PagedResultDto<PracticeRecordDto>>> GetListAsync(PracticeRecordQueryDto input);

    /// <summary>回顾单局练习结果（含逐题对错与答案）</summary>
    Task<BaseOutput<PracticeResultDto>> GetResultAsync(Guid id);

    /// <summary>开始一局练习</summary>
    Task<BaseOutput<PracticeStartDto>> StartAsync(StartPracticeInputDto input);

    /// <summary>提交单题作答，服务端判定后立刻返回对错与当前连对数（游戏即时反馈用）</summary>
    Task<BaseOutput<PracticeItemResultDto>> SubmitItemAsync(SubmitPracticeItemInputDto input);

    /// <summary>交卷结算</summary>
    Task<BaseOutput<PracticeResultDto>> SubmitAsync(Guid id, SubmitPracticeInputDto input);

    /// <summary>删除练习记录</summary>
    Task<BaseOutput> DeleteAsync(Guid id);
}
