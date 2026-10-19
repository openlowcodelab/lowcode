using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 数学专项·单位换算。
/// 换算关系按难易分级录入，低关卡只出十进制/百进制与升毫分这类基础进率，
/// 高关卡才出现平方千米的万进率、反向换算（小单位化大单位）和复名数（2 吨 300 千克 = 多少千克）。
/// 1~3 关四选一，主要考进率直觉（干扰项就是"多写一个 0 / 少写一个 0"这类真实错法）；4 关起改为写得数。
/// </summary>
public class UnitConvertGenerator : IPracticeItemGenerator
{
    private record Rate(string From, string To, long Factor, int MinLevel);

    private static readonly Rate[] Rates =
    [
        new("米", "分米", 10, 1),
        new("分米", "厘米", 10, 1),
        new("厘米", "毫米", 10, 1),
        new("米", "厘米", 100, 1),
        new("元", "角", 10, 1),
        new("角", "分", 10, 1),
        new("年", "月", 12, 1),
        new("元", "分", 100, 2),
        new("千米", "米", 1000, 2),
        new("千克", "克", 1000, 2),
        new("升", "毫升", 1000, 2),
        new("时", "分", 60, 2),
        new("分", "秒", 60, 2),
        new("吨", "千克", 1000, 3),
        new("日", "时", 24, 3),
        new("平方米", "平方分米", 100, 3),
        new("平方分米", "平方厘米", 100, 3),
        new("立方米", "立方分米", 1000, 4),
        new("立方分米", "立方厘米", 1000, 4),
        new("平方米", "平方厘米", 10000, 4),
        new("世纪", "年", 100, 4),
        new("公顷", "平方米", 10000, 5),
        new("平方千米", "公顷", 100, 5),
        new("克", "毫克", 1000, 5),
        new("平方千米", "平方米", 1000000, 6),
        new("吨", "克", 1000000, 6)
    ];

    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.UnitConvert;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Math;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var safeLevel = Math.Clamp(level, 1, 6);
        var pool = Rates.Where(r => r.MinLevel <= safeLevel).ToList();
        if (pool.Count == 0) pool = [Rates[0]];

        var items = new List<GeneratedPracticeItem>(count);
        for (var i = 0; i < count; i++)
        {
            var rate = pool[Random.Shared.Next(pool.Count)];

            var (text, answer) = NextQuestion(rate, safeLevel);
            if (safeLevel <= 3)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    $"在空格里填上合适的数：{text}",
                    BuildOptions(answer, rate),
                    answer.ToString(),
                    $"进率：1 {rate.From} = {rate.Factor} {rate.To}"));
            }
            else
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    $"在空格里填上合适的数（只写得数）：{text}",
                    [],
                    $"{answer}|{answer}{rate.To}",
                    $"进率：1 {rate.From} = {rate.Factor} {rate.To}"));
            }
        }

        return items;
    }

    private static (string Text, long Answer) NextQuestion(Rate rate, int level)
    {
        // 复名数：先有几个大单位再补几个小单位，合并成小单位名数
        if (level >= 5 && rate.Factor >= 100 && Random.Shared.NextDouble() < 0.3)
        {
            var big = Random.Shared.Next(2, 9);
            var small = Random.Shared.Next(1, (int)Math.Min(rate.Factor, 900)) * SmallStep(rate.Factor);
            if (small < rate.Factor)
            {
                return ($"{big} {rate.From} {small} {rate.To} = ____ {rate.To}", big * rate.Factor + small);
            }
        }

        // 反向换算：小单位化大单位，取值保证能整除，避免出现小数
        if (level >= 4 && Random.Shared.NextDouble() < 0.4)
        {
            var times = Random.Shared.Next(2, 10);
            return ($"{times * rate.Factor} {rate.To} = ____ {rate.From}", times);
        }

        var value = rate.Factor >= 10000 ? Random.Shared.Next(1, 5) : Random.Shared.Next(2, 13);
        return ($"{value} {rate.From} = ____ {rate.To}", value * rate.Factor);
    }

    private static int SmallStep(long factor) => factor switch
    {
        >= 1000000 => 100000,
        >= 10000 => 1000,
        >= 1000 => 100,
        _ => 10
    };

    /// <summary>
    /// 干扰项用"多一个 0 / 少一个 0"和"当成加法"这几类真实错法，而不是随便编数字
    /// </summary>
    private static List<string> BuildOptions(long answer, Rate rate)
    {
        var candidates = new List<long> { answer, answer * 10, answer * 100 };

        if (answer % 10 == 0) candidates.Add(answer / 10);
        if (answer % 100 == 0) candidates.Add(answer / 100);
        candidates.Add(answer + rate.Factor);

        var options = new List<long>();
        foreach (var candidate in candidates.Where(x => x > 0).OrderBy(_ => Random.Shared.Next()))
        {
            if (options.Count < 4 && !options.Contains(candidate)) options.Add(candidate);
        }

        for (var extra = 1; options.Count < 4; extra++)
        {
            var candidate = answer + extra * Math.Max(1, rate.Factor / 10);
            if (!options.Contains(candidate)) options.Add(candidate);
        }

        return options.OrderBy(_ => Random.Shared.Next()).Select(x => x.ToString()).ToList();
    }
}
