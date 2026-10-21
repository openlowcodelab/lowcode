using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 一次出题请求。年级决定内容范围，关卡决定题型深浅，
/// Keyword 与 Mode 只有部分游戏用到（飞花令要令字、听力要模式），其余游戏忽略即可。
/// </summary>
/// <param name="Grade">年级原文，如「三年级」</param>
/// <param name="Level">关卡，1 起，越大越难</param>
/// <param name="Count">题目数量</param>
/// <param name="Keyword">学生自输的关键字或词（飞花令）</param>
/// <param name="Mode">模式值（听力的 word/sentence/passage）</param>
public record PracticeGenerateRequest(string Grade, int Level, int Count, string? Keyword, string? Mode);

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
    /// <summary>需要浏览器朗读的文本，听力与对话题使用</summary>
    public string? AudioText { get; init; }

    /// <summary>
    /// 宽松判定：学生作答里只要包含参考答案就算对。
    /// 英文用于对话（自己组织应答，关键词说到即对）；中文用于默写与接句
    /// （孩子把整联都写下来时，「欲穷千里目，更上一层楼」也该算对「更上一层楼」）。
    /// 拼写、挖字类题目不能用，否则错字也能蒙混过关。
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

    /// <summary>生成一局的题目</summary>
    List<GeneratedPracticeItem> Generate(PracticeGenerateRequest request);
}

/// <summary>
/// 游戏目录：卡片展示与可选项参数。
/// 趣味练习放语文的诗词类玩法，专项练习放英语的词、句、对话定向刷题。
/// </summary>
internal static class PracticeGameCatalog
{
    public static List<PracticeGameDto> All { get; } =
    [
        new()
        {
            GameType = PracticeGameTypeEnum.ChinesePoem,
            Subject = EducationSubjectEnum.Chinese,
            Category = PracticeCategoryEnum.Fun,
            Name = "诗词填空",
            Icon = "🏮",
            Description = "按年级选诗出题：一二年级认读小诗，三四年级起挖字、默写、辨出处，关卡越高提示越少",
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
            Description = "自己输入一个关键字或词，把含它的课内诗句填空写出来，高关卡要自己想一句",
            CountOptions = [8, 10, 15],
            MaxLevel = 6,
            NeedsKeyword = true,
            KeywordHint = "如：月、花、春风"
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishWord,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Topic,
            Name = "单词闯关",
            Icon = "🔤",
            Description = "看中文选英文、看中文拼单词，按年级切换词库",
            CountOptions = [8, 10, 15],
            MaxLevel = 6
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishListening,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Topic,
            Name = "听力练习",
            Icon = "🎧",
            Description = "分「单词 / 句子 / 短文」三种模式，浏览器朗读，听音选意，关卡越高越要听写",
            CountOptions = [6, 8, 10],
            MaxLevel = 6,
            Modes =
            [
                new() { Value = EnglishListeningGenerator.ModeWord, Name = "单词" },
                new() { Value = EnglishListeningGenerator.ModeSentence, Name = "句子" },
                new() { Value = EnglishListeningGenerator.ModePassage, Name = "短文" }
            ]
        },
        new()
        {
            GameType = PracticeGameTypeEnum.EnglishDialogue,
            Subject = EducationSubjectEnum.English,
            Category = PracticeCategoryEnum.Topic,
            Name = "对话练习",
            Icon = "💬",
            Description = "听上句后按词填空写应答，关键词说到即算对，高关卡不再给中文提示",
            CountOptions = [6, 8, 10],
            MaxLevel = 6
        }
    ];

    public static List<PracticeGameDto> OfCategory(PracticeCategoryEnum category) =>
        All.Where(g => g.Category == category).ToList();

    public static PracticeGameDto Get(PracticeGameTypeEnum gameType) =>
        All.FirstOrDefault(g => g.GameType == gameType) ?? All[0];
}
