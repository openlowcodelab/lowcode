using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Education.Application.Contracts;

/// <summary>
/// 试卷 DTO
/// </summary>
public class ExamPaperDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public EducationSubjectEnum Subject { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string? Term { get; set; }

    /// <summary>试卷类型：普通练习卷或单元测试卷</summary>
    public PaperKindEnum Kind { get; set; }

    /// <summary>单元名称，如「第三单元」</summary>
    public string? Unit { get; set; }

    /// <summary>教材版本，如「人教版」</summary>
    public string? Textbook { get; set; }

    public int DurationMinutes { get; set; }
    public decimal TotalScore { get; set; }
    public PaperStatusEnum Status { get; set; }
    public PaperSourceEnum Source { get; set; }
    public string? Description { get; set; }
    public string? AiModel { get; set; }
    /// <summary>AI 生成时下发给模型的完整命题要求</summary>
    public string? AiPrompt { get; set; }
    /// <summary>命题状态，AI 生成为异步任务，生成中还没有题目</summary>
    public PaperGenerateStateEnum GenerateState { get; set; }
    /// <summary>生成失败原因</summary>
    public string? GenerateError { get; set; }
    public int QuestionCount { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 选择题选项
/// </summary>
public class QuestionOptionDto
{
    public string Key { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// 试题 DTO
/// </summary>
public class ExamQuestionDto
{
    public Guid Id { get; set; }
    public string? Section { get; set; }
    public QuestionTypeEnum Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<QuestionOptionDto> Options { get; set; } = [];
    public string Answer { get; set; } = string.Empty;
    public string? Analysis { get; set; }
    public decimal Score { get; set; }
    public int Sort { get; set; }
}

/// <summary>
/// 试卷详情（含题目）
/// </summary>
public class ExamPaperDetailDto : ExamPaperDto
{
    public List<ExamQuestionDto> Questions { get; set; } = [];
}

/// <summary>
/// 试卷分页查询条件
/// </summary>
public class ExamPaperQueryDto : PagedAndSortedResultRequestDto
{
    /// <summary>标题模糊查询</summary>
    public string? Filter { get; set; }
    public EducationSubjectEnum? Subject { get; set; }
    public string? Grade { get; set; }
    public PaperStatusEnum? Status { get; set; }

    /// <summary>只看普通练习卷或单元测试卷</summary>
    public PaperKindEnum? Kind { get; set; }
}

/// <summary>
/// 试卷生成入参
/// </summary>
public class GeneratePaperInputDto
{
    /// <summary>学科</summary>
    [Required(ErrorMessage = "请选择学科")]
    public EducationSubjectEnum Subject { get; set; }

    /// <summary>
    /// 试卷类型：普通练习卷按知识点自由命题；
    /// 单元测试卷按 53 天天练的单元卷体例组卷（固定大题结构与分值分配），只能走 AI。
    /// </summary>
    public PaperKindEnum Kind { get; set; } = PaperKindEnum.Normal;

    /// <summary>单元名称，单元测试卷必填，如「第三单元」</summary>
    [StringLength(32, ErrorMessage = "单元名称长度不能超过 32")]
    public string? Unit { get; set; }

    /// <summary>教材版本，如「人教版」「部编版」</summary>
    [StringLength(32, ErrorMessage = "教材版本长度不能超过 32")]
    public string? Textbook { get; set; }

    /// <summary>年级，如 "四年级"</summary>
    [Required(ErrorMessage = "请填写年级")]
    public string Grade { get; set; } = string.Empty;

    /// <summary>学期，如 "上学期"</summary>
    public string? Term { get; set; }

    /// <summary>难度</summary>
    public DifficultyEnum Difficulty { get; set; } = DifficultyEnum.Normal;

    /// <summary>知识点范围，多个用顿号分隔，留空则由模型按年级常见考点命题</summary>
    public string? KnowledgePoints { get; set; }

    /// <summary>题目数量</summary>
    [Range(5, 30, ErrorMessage = "题目数量需在 5~30 之间")]
    public int QuestionCount { get; set; } = 15;

    /// <summary>建议答题时长（分钟）</summary>
    [Range(20, 180, ErrorMessage = "答题时长需在 20~180 分钟之间")]
    public int DurationMinutes { get; set; } = 60;

    /// <summary>生成方式：AI 生成或内置样卷</summary>
    public PaperSourceEnum Source { get; set; } = PaperSourceEnum.Ai;
}

/// <summary>
/// 试卷基本信息更新入参
/// </summary>
public class UpdateExamPaperDto
{
    [Required(ErrorMessage = "试卷标题不能为空")]
    public string Title { get; set; } = string.Empty;

    public int DurationMinutes { get; set; }

    public string? Description { get; set; }

    public PaperStatusEnum Status { get; set; }
}

/// <summary>
/// 试卷发布状态变更入参
/// </summary>
public class PublishPaperInputDto
{
    public PaperStatusEnum Status { get; set; }
}

/// <summary>
/// 按学科统计的试卷数量
/// </summary>
public class ExamPaperStatDto
{
    public EducationSubjectEnum Subject { get; set; }
    public int PaperCount { get; set; }
    public int AnswerCount { get; set; }
    public decimal AvgAccuracyRate { get; set; }
}
