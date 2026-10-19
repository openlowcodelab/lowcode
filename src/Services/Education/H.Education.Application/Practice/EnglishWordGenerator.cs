using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 英语单词闯关出题器：按年级学段从内置词库取词，
/// 低关卡以「中英互选」认读为主，关卡越高越多地要求拼写单词。
/// </summary>
public class EnglishWordGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.EnglishWord;

    public EducationSubjectEnum Subject => EducationSubjectEnum.English;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var bank = PracticeContentBanks.EnglishWords[Math.Clamp(EducationGrades.Band(grade), 0, 2)];
        var picked = bank.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
        var typingRatio = Math.Min(0.6, 0.12 * Math.Clamp(level, 1, 6));

        var items = new List<GeneratedPracticeItem>(picked.Count);
        for (var i = 0; i < picked.Count; i++)
        {
            var (word, meaning) = picked[i];
            var others = bank
                .Where(entry => entry.Word != word)
                .OrderBy(_ => Random.Shared.Next())
                .Take(3)
                .ToList();

            if (Random.Shared.NextDouble() < typingRatio)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    $"根据中文意思写出英文单词：{meaning}",
                    [],
                    word,
                    $"首字母是 {char.ToUpperInvariant(word[0])}，一共 {word.Length} 个字母"));
            }
            else if (i % 2 == 0)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    $"「{meaning}」对应的英文单词是？",
                    Shuffle(others.Select(entry => entry.Word).Append(word)),
                    word,
                    null));
            }
            else
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    $"单词 {word} 的中文意思是？",
                    Shuffle(others.Select(entry => entry.Meaning).Append(meaning)),
                    meaning,
                    null));
            }
        }

        return items;
    }

    private static List<string> Shuffle(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
