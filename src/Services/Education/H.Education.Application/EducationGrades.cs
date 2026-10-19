using System.Text.RegularExpressions;

namespace H.Education.Application;

/// <summary>
/// 年级归一化：把 "三年级"、"七年级"、"Grade 5" 等写法折算成 1~9 与学段
/// </summary>
internal static class EducationGrades
{
    private static readonly char[] ChineseDigits = "一二三四五六七八九".ToCharArray();

    /// <summary>解析年级数字，无法识别时返回 0</summary>
    public static int Parse(string? grade)
    {
        if (string.IsNullOrWhiteSpace(grade)) return 0;

        var match = Regex.Match(grade, @"\d+");
        if (match.Success && int.TryParse(match.Value, out var digit)) return digit;

        // 高一/高二/高三 必须先于「一二三」判断，否则 "高一" 会被认成一年级
        var highSchool = new[] { "高一", "高二", "高三" };
        for (var i = 0; i < highSchool.Length; i++)
        {
            if (grade.Contains(highSchool[i], StringComparison.Ordinal)) return 10 + i;
        }

        for (var i = 0; i < ChineseDigits.Length; i++)
        {
            if (grade.Contains(ChineseDigits[i])) return i + 1;
        }

        return 0;
    }

    /// <summary>学段：1~3 为低年级，4~6 为中年级，7 以上为高年级</summary>
    public static int Band(string? grade) => Parse(grade) switch
    {
        0 => 2,
        <= 3 => 0,
        <= 6 => 1,
        _ => 2
    };

    public static string BandName(int band) => band switch
    {
        0 => "低年级",
        1 => "中年级",
        _ => "高年级"
    };
}
