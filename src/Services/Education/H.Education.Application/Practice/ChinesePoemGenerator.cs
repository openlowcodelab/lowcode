using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 语文诗词填空出题器：从必背古诗库出题。
/// 第 1~2 关只考「选下句」，第 3 关起加入按出处默写，第 4 关起加入「判断出处」。
/// </summary>
public class ChinesePoemGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.ChinesePoem;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Chinese;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var bank = PracticeContentBanks.Poems;
        var picked = bank.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
        var safeLevel = Math.Clamp(level, 1, 6);

        var items = new List<GeneratedPracticeItem>(picked.Count);
        foreach (var poem in picked)
        {
            var others = bank
                .Where(entry => entry.Title != poem.Title)
                .OrderBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();

            var roll = Random.Shared.NextDouble();

            if (safeLevel >= 3 && roll < 0.15 * (safeLevel - 2))
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    $"默写下一句：{poem.First}，____",
                    [],
                    poem.Second,
                    $"{poem.Author}《{poem.Title}》"));
            }
            else if (safeLevel >= 4 && roll > 0.8)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    $"「{poem.First}，{poem.Second}」出自哪首古诗？",
                    Shuffle(others.Select(entry => entry.Title).Append(poem.Title)),
                    poem.Title,
                    poem.Author));
            }
            else
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    $"补全诗句：{poem.First}，____",
                    Shuffle(others.Select(entry => entry.Second).Append(poem.Second)),
                    poem.Second,
                    $"出自{poem.Author}《{poem.Title}》"));
            }
        }

        return items;
    }

    private static List<string> Shuffle(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
