using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Education.Application.Contracts;

/// <summary>
/// 趣味练习游戏信息
/// </summary>
public class PracticeGameDto
{
    public PracticeGameTypeEnum GameType { get; set; }

    /// <summary>所属分区：趣味练习还是专项练习</summary>
    public PracticeCategoryEnum Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public EducationSubjectEnum Subject { get; set; }
    /// <summary>可选题数</summary>
    public List<int> CountOptions { get; set; } = [];
    /// <summary>关卡数</summary>
    public int MaxLevel { get; set; }
}

/// <summary>
/// 练习题目（作答视图，不含答案）
/// </summary>
public class PracticeQuestionDto
{
    public Guid Id { get; set; }
    public int Sort { get; set; }
    public PracticeInputTypeEnum InputType { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string? Hint { get; set; }

    /// <summary>听力与对话题的朗读文本，由浏览器合成语音播放</summary>
    public string? AudioText { get; set; }

    /// <summary>
    /// 需要几个空格（对话题按参考答案的词数给空格，一词一道横线）；
    /// 0 表示用单个输入框（听写拼单词、诗句默写等）。
    /// </summary>
    public int BlankCount { get; set; }
}

/// <summary>
/// 开始练习返回
/// </summary>
public class PracticeStartDto
{
    public Guid SessionId { get; set; }
    public PracticeGameTypeEnum GameType { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int TotalCount { get; set; }
    public List<PracticeQuestionDto> Questions { get; set; } = [];
}

/// <summary>
/// 开始练习入参
/// </summary>
public class StartPracticeInputDto
{
    [Required(ErrorMessage = "请选择练习游戏")]
    public PracticeGameTypeEnum GameType { get; set; }

    /// <summary>年级，题目按年级与关卡生成</summary>
    [Required(ErrorMessage = "请选择年级")]
    public string Grade { get; set; } = string.Empty;

    /// <summary>学生ID，可选；传入时姓名从学生档案带出</summary>
    public Guid? StudentId { get; set; }

    /// <summary>关卡（1 起）</summary>
    [Range(1, 9, ErrorMessage = "关卡需在 1~9 之间")]
    public int Level { get; set; } = 1;

    /// <summary>题目数量</summary>
    [Range(5, 20, ErrorMessage = "题目数量需在 5~20 之间")]
    public int Count { get; set; } = 10;
}

/// <summary>
/// 单题练习作答
/// </summary>
public class PracticeAnswerInputDto
{
    public Guid ItemId { get; set; }
    public string? Answer { get; set; }
}

/// <summary>
/// 练习交卷入参
/// </summary>
public class SubmitPracticeInputDto
{
    public List<PracticeAnswerInputDto> Items { get; set; } = [];

    /// <summary>用时（秒）</summary>
    public int DurationSeconds { get; set; }
}

/// <summary>
/// 单题交卷入参（游戏即时反馈）
/// </summary>
public class SubmitPracticeItemInputDto
{
    public Guid SessionId { get; set; }
    public Guid ItemId { get; set; }
    public string? Answer { get; set; }
}

/// <summary>
/// 单题判定结果
/// </summary>
public class PracticeItemResultDto
{
    public Guid ItemId { get; set; }
    public bool IsCorrect { get; set; }

    /// <summary>正确答案，判定后下发用于即时讲解</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>当前连对次数</summary>
    public int Combo { get; set; }

    /// <summary>本局已判题数</summary>
    public int AnsweredCount { get; set; }

    /// <summary>本局已答对题数</summary>
    public int CorrectCount { get; set; }
}

/// <summary>
/// 练习逐题结果
/// </summary>
public class PracticeResultItemDto
{
    public Guid Id { get; set; }
    public int Sort { get; set; }
    public PracticeInputTypeEnum InputType { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<string> Options { get; set; } = [];
    public string? Hint { get; set; }
    public string? StudentAnswer { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    /// <summary>听力与对话题的朗读原文，结算时对照</summary>
    public string? AudioText { get; set; }
}

/// <summary>
/// 练习成绩
/// </summary>
public class PracticeResultDto
{
    public Guid SessionId { get; set; }
    public PracticeGameTypeEnum GameType { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Level { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int CorrectCount { get; set; }
    public int Score { get; set; }
    public int MaxCombo { get; set; }
    public decimal AccuracyRate { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime FinishTime { get; set; }
    public List<PracticeResultItemDto> Items { get; set; } = [];
}

/// <summary>
/// 练习记录 DTO
/// </summary>
public class PracticeRecordDto
{
    public Guid Id { get; set; }
    public PracticeGameTypeEnum GameType { get; set; }
    public EducationSubjectEnum Subject { get; set; }
    public string Grade { get; set; } = string.Empty;
    public int Level { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int CorrectCount { get; set; }
    public int Score { get; set; }
    public int MaxCombo { get; set; }
    public int DurationSeconds { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 练习记录分页查询条件
/// </summary>
public class PracticeRecordQueryDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public PracticeGameTypeEnum? GameType { get; set; }

    /// <summary>只看某个分区（趣味练习 / 专项练习）的记录</summary>
    public PracticeCategoryEnum? Category { get; set; }
}
