using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Education.Application.Contracts;

/// <summary>
/// 答卷 DTO
/// </summary>
public class AnswerSheetDto
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public string PaperTitle { get; set; } = string.Empty;
    public Guid? StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public AnswerSheetStatusEnum Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public int DurationMinutes { get; set; }
    public int DurationSeconds { get; set; }
    public decimal FullScore { get; set; }
    public decimal ObjectiveScore { get; set; }
    public decimal SubjectiveScore { get; set; }
    public decimal TotalScore { get; set; }
    public int QuestionCount { get; set; }
    public int CorrectCount { get; set; }
    public decimal AccuracyRate { get; set; }
    public string? GradeNote { get; set; }
}

/// <summary>
/// 答题过程中的题目（不含参考答案）
/// </summary>
public class AnswerQuestionDto
{
    public Guid QuestionId { get; set; }
    public string? Section { get; set; }
    public QuestionTypeEnum Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = [];
    public decimal Score { get; set; }
    public int Sort { get; set; }
    public string? StudentAnswer { get; set; }
}

/// <summary>
/// 开始答题返回：答卷 + 题目
/// </summary>
public class AnswerStartDto
{
    public AnswerSheetDto Sheet { get; set; } = new();
    public List<AnswerQuestionDto> Questions { get; set; } = [];
}

/// <summary>
/// 逐题批改结果
/// </summary>
public class AnswerDetailDto
{
    public Guid Id { get; set; }
    public Guid QuestionId { get; set; }
    public int Sort { get; set; }
    public QuestionTypeEnum QuestionType { get; set; }
    public string QuestionContent { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = [];
    public string? StudentAnswer { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Analysis { get; set; }
    public decimal Score { get; set; }
    public decimal AwardedScore { get; set; }
    public bool? IsCorrect { get; set; }
    public JudgeModeEnum JudgeMode { get; set; }
    public string? Feedback { get; set; }
}

/// <summary>
/// 批改结果（答卷概览 + 逐题明细）
/// </summary>
public class AnswerResultDto : AnswerSheetDto
{
    public List<AnswerDetailDto> Details { get; set; } = [];
}

/// <summary>
/// 答卷分页查询条件
/// </summary>
public class AnswerSheetQueryDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? PaperId { get; set; }
    public AnswerSheetStatusEnum? Status { get; set; }
}

/// <summary>
/// 开始答题入参
/// </summary>
public class StartAnswerInputDto
{
    [Required(ErrorMessage = "请选择试卷")]
    public Guid PaperId { get; set; }

    /// <summary>学生ID，可选；传入时姓名从学生档案带出</summary>
    public Guid? StudentId { get; set; }
}

/// <summary>
/// 单题作答
/// </summary>
public class AnswerInputItemDto
{
    public Guid QuestionId { get; set; }

    /// <summary>作答内容，多选题选项字母连续书写（如 "AB"）</summary>
    public string? Answer { get; set; }
}

/// <summary>
/// 保存答题进度入参
/// </summary>
public class SaveAnswerInputDto
{
    public List<AnswerInputItemDto> Items { get; set; } = [];
}

/// <summary>
/// 交卷入参
/// </summary>
public class SubmitAnswerInputDto
{
    public List<AnswerInputItemDto> Items { get; set; } = [];

    /// <summary>答题用时（秒）</summary>
    public int DurationSeconds { get; set; }
}
