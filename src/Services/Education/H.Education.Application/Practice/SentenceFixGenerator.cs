using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 语文专项·修改病句。
/// 两种形态：「判断病因」（四选一的病因类别）与「动手改句」（输入修改后的句子）。
/// 低关卡以判断病因为主，第 4 关起以动手改句为主；改句只认参考答案里的写法（忽略标点与空格差异），
/// 所以同一句的多种正确改法都用 | 列在答案里。
/// </summary>
public class SentenceFixGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.SentenceFix;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Chinese;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var bank = PracticeContentBanks.SickSentences;
        var causes = PracticeContentBanks.SickSentenceCauses;
        var safeLevel = Math.Clamp(level, 1, 6);
        var fixRatio = safeLevel <= 3 ? 0.3 : 0.7;

        var items = new List<GeneratedPracticeItem>(count);
        for (var i = 0; i < count; i++)
        {
            var picked = bank[Random.Shared.Next(bank.Length)];
            var wantFix = Random.Shared.NextDouble() < fixRatio;

            if (wantFix)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    $"读出下面的病句，把修改后的句子完整写下来：\n{picked.Wrong}",
                    [],
                    picked.Fixed,
                    $"病因：{picked.Cause}")
                { LooseMatch = false });

                continue;
            }

            var distract = causes.Where(c => c != picked.Cause)
                .OrderBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();

            items.Add(new GeneratedPracticeItem(
                PracticeInputTypeEnum.Choice,
                $"下面的句子犯了什么毛病？\n{picked.Wrong}",
                Shuffle(distract.Append(picked.Cause).ToList()),
                picked.Cause,
                $"改好后是：{FirstVariant(picked.Fixed)}"));
        }

        return items;
    }

    private static string FirstVariant(string fixedSentence) =>
        fixedSentence.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).First();

    private static List<string> Shuffle(List<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
