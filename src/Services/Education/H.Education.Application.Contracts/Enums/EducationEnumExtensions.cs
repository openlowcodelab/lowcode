namespace H.Education.Application.Contracts;

/// <summary>
/// 枚举展示文案
/// </summary>
public static class EducationEnumExtensions
{
    public static string GetSubjectName(this EducationSubjectEnum subject) => subject switch
    {
        EducationSubjectEnum.Chinese => "语文",
        EducationSubjectEnum.Math => "数学",
        EducationSubjectEnum.English => "英语",
        _ => "未知"
    };

    public static string GetQuestionTypeName(this QuestionTypeEnum type) => type switch
    {
        QuestionTypeEnum.SingleChoice => "单选题",
        QuestionTypeEnum.MultipleChoice => "多选题",
        QuestionTypeEnum.TrueFalse => "判断题",
        QuestionTypeEnum.FillBlank => "填空题",
        QuestionTypeEnum.ShortAnswer => "简答题",
        QuestionTypeEnum.Calculation => "计算题",
        QuestionTypeEnum.Composition => "作文",
        _ => "未知题型"
    };

    /// <summary>是否为可机器精确判定的客观题</summary>
    public static bool IsObjective(this QuestionTypeEnum type) => type switch
    {
        QuestionTypeEnum.SingleChoice or QuestionTypeEnum.TrueFalse
            or QuestionTypeEnum.FillBlank or QuestionTypeEnum.Calculation => true,
        QuestionTypeEnum.MultipleChoice => true,
        _ => false
    };

    public static string GetStatusName(this PaperStatusEnum status) =>
        status == PaperStatusEnum.Published ? "已发布" : "草稿";

    public static string GetKindName(this PaperKindEnum kind) =>
        kind == PaperKindEnum.UnitTest ? "单元测试卷" : "普通练习卷";

    public static string GetGenerateStateName(this PaperGenerateStateEnum state) => state switch
    {
        PaperGenerateStateEnum.Generating => "生成中",
        PaperGenerateStateEnum.Failed => "生成失败",
        _ => "已完成"
    };

    public static string GetStatusName(this AnswerSheetStatusEnum status) => status switch
    {
        AnswerSheetStatusEnum.Answering => "答题中",
        AnswerSheetStatusEnum.Saved => "已保存",
        AnswerSheetStatusEnum.Graded => "已批改",
        _ => "未知"
    };

    public static string GetGameName(this PracticeGameTypeEnum game) => game switch
    {
        PracticeGameTypeEnum.MathSprint => "口算闯关",
        PracticeGameTypeEnum.EnglishWord => "单词闯关",
        PracticeGameTypeEnum.ChinesePoem => "诗词填空",
        PracticeGameTypeEnum.EnglishListening => "听力练习",
        PracticeGameTypeEnum.EnglishDialogue => "对话练习",
        PracticeGameTypeEnum.SentenceFix => "修改病句",
        PracticeGameTypeEnum.UnitConvert => "单位换算",
        PracticeGameTypeEnum.FeihuaLing => "飞花令",
        _ => "练习"
    };

    public static string GetGenderName(this GenderEnum gender) => gender switch
    {
        GenderEnum.Male => "男",
        GenderEnum.Female => "女",
        _ => "未填写"
    };

    public static string GetStageName(this SchoolStageEnum stage) => stage switch
    {
        SchoolStageEnum.Primary => "小学",
        SchoolStageEnum.JuniorHigh => "初中",
        SchoolStageEnum.SeniorHigh => "高中",
        _ => "未知学段"
    };

    public static string GetSubjectName(this TextbookSubjectEnum subject) => subject switch
    {
        TextbookSubjectEnum.Chinese => "语文",
        TextbookSubjectEnum.Math => "数学",
        TextbookSubjectEnum.English => "英语",
        TextbookSubjectEnum.EthicsAndRuleOfLaw => "道德与法治",
        TextbookSubjectEnum.Physics => "物理",
        TextbookSubjectEnum.Chemistry => "化学",
        TextbookSubjectEnum.Biology => "生物学",
        TextbookSubjectEnum.History => "历史",
        _ => "其它学科"
    };

    public static string GetVolumeName(this TextbookVolumeEnum volume) => volume switch
    {
        TextbookVolumeEnum.First => "上册",
        TextbookVolumeEnum.Second => "下册",
        TextbookVolumeEnum.Whole => "全一册",
        _ => "未区分册次"
    };

    public static string GetResourceTypeName(this TeachingResourceTypeEnum type) => type switch
    {
        TeachingResourceTypeEnum.VideoCourse => "视频课程",
        TeachingResourceTypeEnum.Courseware => "课件",
        TeachingResourceTypeEnum.InstructionDesign => "教学设计",
        TeachingResourceTypeEnum.LearningTaskSheet => "学习任务单",
        TeachingResourceTypeEnum.AfterClassExercise => "课后练习",
        _ => "其它资源"
    };

    /// <summary>资源类型图标，页面按类型渲染时使用</summary>
    public static string GetResourceTypeIcon(this TeachingResourceTypeEnum type) => type switch
    {
        TeachingResourceTypeEnum.VideoCourse => "📺",
        TeachingResourceTypeEnum.Courseware => "📊",
        TeachingResourceTypeEnum.InstructionDesign => "📐",
        TeachingResourceTypeEnum.LearningTaskSheet => "📋",
        TeachingResourceTypeEnum.AfterClassExercise => "✏️",
        _ => "📎"
    };

    /// <summary>平台"课程教学"板块的五类资源，导入内置目录时逐类展开</summary>
    public static TeachingResourceTypeEnum[] AllResourceTypes { get; } =
    [
        TeachingResourceTypeEnum.VideoCourse,
        TeachingResourceTypeEnum.Courseware,
        TeachingResourceTypeEnum.InstructionDesign,
        TeachingResourceTypeEnum.LearningTaskSheet,
        TeachingResourceTypeEnum.AfterClassExercise
    ];
}
