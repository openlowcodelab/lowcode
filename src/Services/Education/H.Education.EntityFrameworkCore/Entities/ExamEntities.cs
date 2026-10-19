using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 试卷实体
/// </summary>
public class ExamPaperEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>试卷标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>学科（对应 Contracts 的 EducationSubjectEnum）</summary>
    public int Subject { get; set; }

    /// <summary>年级，如 "三年级"、"八年级"</summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>学期，如 "上学期"</summary>
    public string? Term { get; set; }

    /// <summary>试卷类型（0 普通练习卷、1 单元测试卷）</summary>
    public int Kind { get; set; }

    /// <summary>单元名称，如 "第三单元"</summary>
    public string? Unit { get; set; }

    /// <summary>教材版本，如 "人教版"</summary>
    public string? Textbook { get; set; }

    /// <summary>建议答题时长（分钟）</summary>
    public int DurationMinutes { get; set; }

    /// <summary>试卷总分（由题目分值累加得出）</summary>
    public decimal TotalScore { get; set; }

    /// <summary>状态（0 草稿、1 已发布）</summary>
    public int Status { get; set; }

    /// <summary>来源（0 AI 生成、1 内置样卷）</summary>
    public int Source { get; set; }

    /// <summary>生成时使用的知识点/命题要求描述</summary>
    public string? Description { get; set; }

    /// <summary>AI 生成时实际下发的命题提示词</summary>
    public string? AiPrompt { get; set; }

    /// <summary>AI 生成时使用的模型名称</summary>
    public string? AiModel { get; set; }

    /// <summary>命题状态（1 生成中、2 已完成、3 失败），内置样卷直接落库为已完成</summary>
    public int GenerateState { get; set; }

    /// <summary>生成失败原因</summary>
    public string? GenerateError { get; set; }

    /// <summary>发起生成时的原始入参 JSON，失败后可原样重试</summary>
    public string? GenerateInput { get; set; }

    /// <summary>题目数量</summary>
    public int QuestionCount { get; set; }
}

/// <summary>
/// 试卷题目实体
/// </summary>
public class ExamQuestionEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属试卷ID</summary>
    public Guid PaperId { get; set; }

    /// <summary>大题名称，如 "一、字词基础"</summary>
    public string? Section { get; set; }

    /// <summary>题型（对应 Contracts 的 QuestionTypeEnum）</summary>
    public int Type { get; set; }

    /// <summary>题干</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>选择题选项 JSON 数组 [{"key":"A","text":"..."}]，非选择题为空</summary>
    public string? OptionsJson { get; set; }

    /// <summary>参考答案，多选与多个填空可接受答案用 | 分隔</summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>答案解析</summary>
    public string? Analysis { get; set; }

    /// <summary>本题分值</summary>
    public decimal Score { get; set; }

    /// <summary>试卷内排序</summary>
    public int Sort { get; set; }

    /// <summary>关联试卷</summary>
    public virtual ExamPaperEntity? Paper { get; set; }
}
