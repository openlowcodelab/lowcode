using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 答卷实体
/// </summary>
public class AnswerSheetEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>试卷ID</summary>
    public Guid PaperId { get; set; }

    /// <summary>试卷标题快照，试卷删除后记录仍可读</summary>
    public string PaperTitle { get; set; } = string.Empty;

    /// <summary>学生ID，删除档案后历史答卷仍按姓名快照可读</summary>
    public Guid? StudentId { get; set; }

    /// <summary>学生姓名</summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>状态（0 答题中、1 已保存、2 已批改）</summary>
    public int Status { get; set; }

    /// <summary>开始答题时间</summary>
    public DateTime StartTime { get; set; }

    /// <summary>交卷时间</summary>
    public DateTime? SubmitTime { get; set; }

    /// <summary>试卷建议答题时长（分钟）</summary>
    public int DurationMinutes { get; set; }

    /// <summary>答题用时（秒）</summary>
    public int DurationSeconds { get; set; }

    /// <summary>试卷总分快照</summary>
    public decimal FullScore { get; set; }

    /// <summary>客观题（自动判定）得分</summary>
    public decimal ObjectiveScore { get; set; }

    /// <summary>主观题（AI 判定）得分</summary>
    public decimal SubjectiveScore { get; set; }

    /// <summary>总得分</summary>
    public decimal TotalScore { get; set; }

    /// <summary>题目总数</summary>
    public int QuestionCount { get; set; }

    /// <summary>答对题数</summary>
    public int CorrectCount { get; set; }

    /// <summary>得分率（0~100）</summary>
    public decimal AccuracyRate { get; set; }

    /// <summary>批改说明，如主观题改用关键词兜底判分时记录原因</summary>
    public string? GradeNote { get; set; }

    /// <summary>关联试卷</summary>
    public virtual ExamPaperEntity? Paper { get; set; }
}

/// <summary>
/// 答卷明细（逐题作答与逐题评分结果）
/// </summary>
public class AnswerDetailEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>答卷ID</summary>
    public Guid SheetId { get; set; }

    /// <summary>题目ID</summary>
    public Guid QuestionId { get; set; }

    /// <summary>题型快照</summary>
    public int QuestionType { get; set; }

    /// <summary>题干快照</summary>
    public string QuestionContent { get; set; } = string.Empty;

    /// <summary>选项快照 JSON，非选择题为空</summary>
    public string? OptionsJson { get; set; }

    /// <summary>学生作答</summary>
    public string? StudentAnswer { get; set; }

    /// <summary>参考答案快照</summary>
    public string CorrectAnswer { get; set; } = string.Empty;

    /// <summary>解析快照，批改后回看时展示</summary>
    public string? Analysis { get; set; }

    /// <summary>本题分值</summary>
    public decimal Score { get; set; }

    /// <summary>本题得分</summary>
    public decimal AwardedScore { get; set; }

    /// <summary>是否正确，未批改时为 null</summary>
    public bool? IsCorrect { get; set; }

    /// <summary>判定方式（0 未判定、1 自动判定、2 AI 判定、3 关键词兜底）</summary>
    public int JudgeMode { get; set; }

    /// <summary>AI 批改评语</summary>
    public string? Feedback { get; set; }

    /// <summary>排序</summary>
    public int Sort { get; set; }

    /// <summary>关联答卷</summary>
    public virtual AnswerSheetEntity? Sheet { get; set; }
}
