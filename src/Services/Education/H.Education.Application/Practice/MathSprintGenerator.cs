using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 数学口算闯关出题器：按关卡生成 20 以内加减到小数运算的口算题，四选一作答。
/// 题目由随机数生成，不依赖题库与 AI，可以无限出题。
/// </summary>
public class MathSprintGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.MathSprint;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Math;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var items = new List<GeneratedPracticeItem>(count);
        var used = new HashSet<string>(StringComparer.Ordinal);

        // 随机出题可能重题，限制重试次数避免极端情况下死循环
        for (var attempt = 0; items.Count < count && attempt < count * 30; attempt++)
        {
            var (text, answer) = NextQuestion(level);
            if (!used.Add(text)) continue;

            items.Add(new GeneratedPracticeItem(
                PracticeInputTypeEnum.Choice,
                text,
                BuildOptions(answer, level),
                Format(answer, level),
                null));
        }

        return items;
    }

    private static (string Text, double Answer) NextQuestion(int level)
    {
        var rnd = Random.Shared;
        return Math.Clamp(level, 1, 9) switch
        {
            1 => Add(rnd, 10, 20),
            2 => Subtract(rnd, 20),
            3 => AddSubtract(rnd, 100),
            4 => Multiply(rnd),
            5 => Divide(rnd),
            6 => MultiplyTwoDigit(rnd),
            7 => DivideTwoDigit(rnd),
            8 => Mixed(rnd),
            _ => DecimalAddSubtract(rnd)
        };
    }

    private static (string, double) Add(Random rnd, int operandMax, int sumMax)
    {
        var a = rnd.Next(1, operandMax + 1);
        var b = rnd.Next(1, Math.Max(2, sumMax - a));
        return ($"{a} + {b} = ?", a + b);
    }

    private static (string, double) Subtract(Random rnd, int max)
    {
        var a = rnd.Next(5, max + 1);
        var b = rnd.Next(1, a);
        return ($"{a} − {b} = ?", a - b);
    }

    private static (string, double) AddSubtract(Random rnd, int max)
    {
        if (rnd.Next(2) == 0)
        {
            var a = rnd.Next(20, max - 20);
            var b = rnd.Next(10, max - a);
            return ($"{a} + {b} = ?", a + b);
        }
        else
        {
            var a = rnd.Next(40, max + 1);
            var b = rnd.Next(10, a);
            return ($"{a} − {b} = ?", a - b);
        }
    }

    private static (string, double) Multiply(Random rnd)
    {
        var a = rnd.Next(2, 10);
        var b = rnd.Next(2, 10);
        return ($"{a} × {b} = ?", a * b);
    }

    private static (string, double) Divide(Random rnd)
    {
        var b = rnd.Next(2, 10);
        var q = rnd.Next(2, 10);
        return ($"{b * q} ÷ {b} = ?", q);
    }

    private static (string, double) MultiplyTwoDigit(Random rnd)
    {
        var a = rnd.Next(12, 50);
        var b = rnd.Next(2, 10);
        return ($"{a} × {b} = ?", a * b);
    }

    private static (string, double) DivideTwoDigit(Random rnd)
    {
        var b = rnd.Next(2, 10);
        var q = rnd.Next(4, 13);
        return ($"{b * q} ÷ {b} = ?", q);
    }

    private static (string, double) Mixed(Random rnd)
    {
        var b = rnd.Next(2, 7);
        var c = rnd.Next(2, 9);
        var a = rnd.Next(5, 40);
        return ($"{a} + {b} × {c} = ?", a + b * c);
    }

    private static (string, double) DecimalAddSubtract(Random rnd)
    {
        var a = rnd.Next(15, 95) / 10.0;
        var b = rnd.Next(5, 45) / 10.0;
        return rnd.Next(2) == 0
            ? ($"{a:0.0} + {b:0.0} = ?", Math.Round(a + b, 1))
            : ($"{a:0.0} − {b:0.0} = ?", Math.Round(a - b, 1));
    }

    /// <summary>
    /// 构造四个选项：正确得数 + 三个由常见错算（少一位、进位漏掉、乘除混淆）得到的干扰项
    /// </summary>
    private static List<string> BuildOptions(double answer, int level)
    {
        var decimals = Math.Clamp(level, 1, 9) == 9;
        var deltas = decimals
            ? new[] { 0.1, -0.1, 0.5, -0.5, 1.0, -1.0 }
            : new[] { 1.0, -1.0, 2.0, -2.0, 10.0, -10.0 };

        var options = new List<string> { Format(answer, level) };
        var shuffled = deltas.OrderBy(_ => Random.Shared.Next()).ToList();

        foreach (var delta in shuffled)
        {
            if (options.Count >= 4) break;

            var candidate = Math.Round(answer + delta, 1);
            if (candidate < 0) continue;

            var text = Format(candidate, level);
            if (!options.Contains(text)) options.Add(text);
        }

        for (var extra = 3; options.Count < 4; extra++)
        {
            var text = Format(Math.Round(answer + extra, 1), level);
            if (!options.Contains(text)) options.Add(text);
        }

        return options.OrderBy(_ => Random.Shared.Next()).ToList();
    }

    private static string Format(double value, int level) =>
        Math.Clamp(level, 1, 9) == 9 ? value.ToString("0.0") : ((int)Math.Round(value)).ToString();
}
