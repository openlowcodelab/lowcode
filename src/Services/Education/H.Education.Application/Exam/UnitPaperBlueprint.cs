using System.Text;
using H.Education.Application.Contracts;

namespace H.Education.Application.Exam;

/// <summary>
/// 单元测试卷蓝图：照 53 天天练单元卷的体例规定大题顺序、题型与分值配比。
/// 整卷一次性输出对慢模型太重（十几道题还带短文，动辄超过十分钟被掐断），所以蓝图按大题拆开，
/// 一个大题一次命题再拼成整卷，模型只能在指定的大题里出题。
/// 难度对应 53 的「基础达标 / 能力提升 / 尖子培优」三档。
/// </summary>
internal static class UnitPaperBlueprint
{
    /// <summary>一个大题：Name 直接写成试题的 section，Requirement 是该大题的题型与分值要求</summary>
    public readonly record struct Section(string Name, string Requirement);

    /// <summary>
    /// 各大题分值是给死的定值而不是区间，四（六）个大题加起来恰好 100 分——
    /// 给区间时模型各题分值之和经常落在 90~96，整卷凑不满 100。
    /// </summary>
    public static List<Section> Sections(EducationSubjectEnum subject) => subject switch
    {
        EducationSubjectEnum.Chinese =>
        [
            new("一、基础知识",
                "给加点字选正确读音（2~3 题）、看拼音写词语（1 题含 4~6 个词）、补全四字词语并选词填空（1~2 题）、按要求改写句子（近反义词、比喻拟人、修改病句，2~3 题）；本大题合计 40 分"),
            new("二、积累与运用",
                "按本单元课文与古诗原文填空（3~4 题），再出 1 题「日积月累」名句填空；本大题合计 15 分"),
            new("三、阅读理解",
                "课内语段 1 篇配 2~3 题、课外短文 1 篇（120~200 字）配 3~4 题；短文要完整写进对应题目的 content（题干含短文与小问），不要单独出「阅读材料」字段，也不要写「见试卷第 X 篇」这类无法独立作答的表述；本大题合计 20 分"),
            new("四、习作",
                "紧扣本单元表达主题出 1 题，answer 写写作要求与立意提示，analysis 写评分要点；本大题合计 25 分"),
        ],
        EducationSubjectEnum.Math =>
        [
            new("一、填空题", "考本单元概念、单位换算、图形与数的基本性质，6~8 题；本大题合计 24 分"),
            new("二、判断题", "针对本单元易错说法设题，2~3 题；本大题合计 6 分"),
            new("三、选择题", "四个选项都要写进 options，3~4 题；本大题合计 12 分"),
            new("四、计算题", "直接写得数、竖式或脱式计算，能简算的要留出简算机会，answer 写得数与关键步骤，4~6 题；本大题合计 24 分"),
            new("五、操作与图形", "画图、求周长或面积、数图形等，用文字描述操作要求，1~2 题；本大题合计 10 分"),
            new("六、解决问题", "用本单元知识解决实际问题，analysis 写完整解题思路，3~4 题；本大题合计 24 分"),
        ],
        EducationSubjectEnum.English =>
        [
            new("一、听力", "题干以「（听老师朗读）」开头，把要朗读的英文句子写在 answer 与 analysis 里，选项用中文或英文短句，3~4 题；本大题合计 20 分"),
            new("二、词汇与语法", "选出不同类单词、看图或根据首字母填词、用所给词的适当形式填空，5~6 题；本大题合计 25 分"),
            new("三、情景交际", "给出上句选应答，或补全对话中的一句，3~4 题；本大题合计 15 分"),
            new("四、阅读理解", "一篇 80~150 词的短文完整写进第一题的 content，其余题在题干标注「阅读短文」，配判断或选择题 4~5 题；本大题合计 25 分"),
            new("五、书面表达", "紧扣本单元话题出 1 题，answer 写参考范文要点，analysis 写评分要点；本大题合计 15 分"),
        ],
        _ =>
        [
            new("一、基础题", "覆盖本单元基本概念与直接运用，5~8 题；本大题合计 55 分"),
            new("二、综合题", "需要一步以上推理或实际情境应用，3~5 题；本大题合计 45 分"),
        ],
    };

    /// <summary>与具体大题无关、整卷都要遵守的学科约束</summary>
    public static string SubjectNote(EducationSubjectEnum subject) => subject switch
    {
        EducationSubjectEnum.Chinese => "只考该单元的课文、词语与句式，不超出课本范围，阅读短文的篇幅适合该年级。",
        EducationSubjectEnum.Math => "计算结果必须真实正确，应用题数据符合生活常理。",
        EducationSubjectEnum.English => "词汇与句式不得超出该年级该单元范围。",
        _ => "题目范围不超出该单元。"
    };

    public static string DifficultyNote(DifficultyEnum difficulty) => difficulty switch
    {
        DifficultyEnum.Easy => "按 53 天天练「基础达标」层次命题：八成题目直接考课本原样知识点，不设陷阱，不出现拓展题。",
        DifficultyEnum.Hard => "按 53 天天练「尖子培优」层次命题：基础题占三成，综合题占五成，本大题里最难的 1~2 题给拓展与压轴。",
        _ => "按 53 天天练「能力提升」层次命题：基础题约占六成，需要一步以上推理的题约占三成，拓展题约占一成。"
    };

    public static string BuildScope(GeneratePaperInputDto input)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(input.Textbook)) sb.Append(input.Textbook.Trim()).Append(' ');
        sb.Append(input.Grade.Trim());
        if (!string.IsNullOrWhiteSpace(input.Term)) sb.Append(input.Term!.Trim());
        sb.Append(string.IsNullOrWhiteSpace(input.Unit) ? "本单元" : input.Unit!.Trim());
        return sb.ToString();
    }
}
