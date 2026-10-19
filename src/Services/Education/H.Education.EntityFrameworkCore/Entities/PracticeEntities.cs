using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 趣味练习对局实体
/// </summary>
public class PracticeSessionEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>游戏类型（对应 Contracts 的 PracticeGameTypeEnum）</summary>
    public int GameType { get; set; }

    /// <summary>学科</summary>
    public int Subject { get; set; }

    /// <summary>年级</summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>关卡（1~9）</summary>
    public int Level { get; set; }

    /// <summary>学生ID，删除档案后记录仍按姓名快照可读</summary>
    public Guid? StudentId { get; set; }

    /// <summary>学生姓名</summary>
    public string StudentName { get; set; } = string.Empty;

    /// <summary>状态（0 进行中、1 已完成）</summary>
    public int Status { get; set; }

    /// <summary>题目总数</summary>
    public int TotalCount { get; set; }

    /// <summary>答对题数</summary>
    public int CorrectCount { get; set; }

    /// <summary>得分</summary>
    public int Score { get; set; }

    /// <summary>最长连对次数</summary>
    public int MaxCombo { get; set; }

    /// <summary>用时（秒）</summary>
    public int DurationSeconds { get; set; }
}

/// <summary>
/// 趣味练习题目实体（服务端持有正确答案，交卷时据此判分）
/// </summary>
public class PracticeItemEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>对局ID</summary>
    public Guid SessionId { get; set; }

    /// <summary>排序</summary>
    public int Sort { get; set; }

    /// <summary>作答方式（0 选择、1 输入）</summary>
    public int InputType { get; set; }

    /// <summary>题干</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>选项 JSON，输入题为空</summary>
    public string? OptionsJson { get; set; }

    /// <summary>正确答案</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>提示（如英语首字母提示、诗词出处）</summary>
    public string? Hint { get; set; }

    /// <summary>需要朗读的英文原文，听力与对话题由浏览器合成语音朗读</summary>
    public string? AudioText { get; set; }

    /// <summary>是否按关键词宽松判定（对话练习学生自己组织句子时用），拼写与默写题为 false</summary>
    public bool LooseMatch { get; set; }

    /// <summary>作答空格数：>0 表示按词逐空填写（对话练习），0 表示单个输入框</summary>
    public int BlankCount { get; set; }

    /// <summary>学生作答</summary>
    public string? StudentAnswer { get; set; }

    /// <summary>是否答对</summary>
    public bool? IsCorrect { get; set; }

    /// <summary>关联对局</summary>
    public virtual PracticeSessionEntity? Session { get; set; }
}
