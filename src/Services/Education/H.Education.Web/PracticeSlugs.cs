using H.Education.Application.Contracts;

namespace H.Education.Web;

/// <summary>
/// 练习游戏的路由短名与枚举互转，让地址可读（/education/practice/math）
/// </summary>
public static class PracticeSlugs
{
    public static string ToSlug(PracticeGameTypeEnum gameType) => gameType switch
    {
        PracticeGameTypeEnum.MathSprint => "math",
        PracticeGameTypeEnum.EnglishWord => "english",
        PracticeGameTypeEnum.ChinesePoem => "poem",
        PracticeGameTypeEnum.FeihuaLing => "feihua",
        PracticeGameTypeEnum.EnglishListening => "listening",
        PracticeGameTypeEnum.EnglishDialogue => "dialogue",
        PracticeGameTypeEnum.SentenceFix => "sentence-fix",
        PracticeGameTypeEnum.UnitConvert => "unit-convert",
        _ => "math"
    };

    public static PracticeGameTypeEnum? FromSlug(string? slug) => slug?.ToLowerInvariant() switch
    {
        "math" => PracticeGameTypeEnum.MathSprint,
        "english" => PracticeGameTypeEnum.EnglishWord,
        "poem" => PracticeGameTypeEnum.ChinesePoem,
        "feihua" => PracticeGameTypeEnum.FeihuaLing,
        "listening" => PracticeGameTypeEnum.EnglishListening,
        "dialogue" => PracticeGameTypeEnum.EnglishDialogue,
        "sentence-fix" => PracticeGameTypeEnum.SentenceFix,
        "unit-convert" => PracticeGameTypeEnum.UnitConvert,
        _ => null
    };
}
