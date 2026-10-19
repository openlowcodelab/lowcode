using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 生成的练习题目（尚未落库）
/// </summary>
public record GeneratedPracticeItem(
    PracticeInputTypeEnum InputType,
    string Content,
    List<string> Options,
    string Answer,
    string? Hint)
{
    /// <summary>需要浏览器朗读的英文原文，听力与对话题使用</summary>
    public string? AudioText { get; init; }

    /// <summary>
    /// 宽松判定：学生自己组织英文应答时，只要参考答案的关键词都出现就算对。
    /// 拼写、默写类题目不能用，否则 "apples" 写错也能蒙混过关。
    /// </summary>
    public bool LooseMatch { get; init; }

    /// <summary>作答空格数：&gt;0 时前端一词一空（对话练习），0 表示单个输入框</summary>
    public int BlankCount { get; init; }
}

/// <summary>
/// 趣味练习出题器。每个游戏一个实现，由 DI 收集后按 GameType 分发。
/// </summary>
public interface IPracticeItemGenerator
{
    PracticeGameTypeEnum GameType { get; }

    /// <summary>学科</summary>
    EducationSubjectEnum Subject { get; }

    /// <summary>
    /// 生成一局的题目
    /// </summary>
    /// <param name="grade">年级，用于选取内容范围</param>
    /// <param name="level">关卡，1 起，越大越难</param>
    /// <param name="count">题目数量</param>
    List<GeneratedPracticeItem> Generate(string grade, int level, int count);
}

/// <summary>
/// 游戏目录：卡片展示与可选项参数
/// </summary>
internal static class PracticeGameCatalog
{
    public static List<PracticeGameDto> All { get; } =
    [
        new()
        {
            GameType = PracticeGameTypeEnum.MathSprint,
            Subject = EducationSubjectEnum.Math,
            Category = PracticeCategoryEnum.Fun,
            Name = "口算闯关",
            Icon = "➗",
            Description = "限时口算，四选一快速作答，连对越多得分越高",
            CountOptions = [10, 15, 20],
            MaxLevel = 9
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishWord,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Fun,
            Name = "单词闯关",
            Icon = "🔤",
            Description = "看中文选英文、看中文拼单词，按年级切换词库",
            CountOptions = [8, 10, 15],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.ChinesePoem,
            Subject = EducationSubjectEnum.Chinese,
            Category = PracticeCategoryEnum.Fun,
            Name = "诗词填空",
            Icon = "🏮",
            Description = "必背古诗接句与默写，答对显示出处",
            CountOptions = [8, 10, 15],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.FeihuaLing,
            Subject = EducationSubjectEnum.Chinese,
            Category = PracticeCategoryEnum.Fun,
            Name = "飞花令",
            Icon = "🌸",
            Description = "抽一个令字，从诗句里找它、辨它排在第几个字，关卡越高越要自己接一句",
            CountOptions = [8, 10, 15],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishListening,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Fun,
            Name = "听力练习",
            Icon = "🎧",
            Description = "每局都混合单词与句子，浏览器朗读，听音选意思或听写拼单词",
            CountOptions = [6, 8, 10],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishDialogue,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Fun,
            Name = "对话练习",
            Icon = "💬",
            Description = "听上句后按词填空写应答，关键词说到即算对，高关卡不再给中文提示",
            CountOptions = [6, 8, 10],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.SentenceFix,
            Subject = EducationSubjectEnum.Chinese,
            Category = PracticeCategoryEnum.Topic,
            Name = "修改病句",
            Icon = "🩺",
            Description = "先判断病因，再动手改句：成分残缺、搭配不当、重复啰嗦、前后矛盾等常见病句",
            CountOptions = [6, 8, 10],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.UnitConvert,
            Subject = EducationSubjectEnum.Math,
            Category = PracticeCategoryEnum.Topic,
            Name = "单位换算",
            Icon = "📏",
            Description = "长度、面积、体积、质量、时间、人民币名数互化，高关卡含反向与复合名数",
            CountOptions = [8, 10, 15],
            MaxLevel = 6
        }
    ];

    public static List<PracticeGameDto> OfCategory(PracticeCategoryEnum category) =>
        All.Where(g => g.Category == category).ToList();

    public static PracticeGameDto Get(PracticeGameTypeEnum gameType) =>
        All.FirstOrDefault(g => g.GameType == gameType) ?? All[0];
}
