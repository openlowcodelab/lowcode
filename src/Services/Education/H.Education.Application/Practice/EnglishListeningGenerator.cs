using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 英语听力练习出题器。
/// 音频由浏览器 Web Speech 合成，服务端只下发要朗读的英文原文，
/// 一律「听英文、选中文意思」或「听英文、拼写单词」，朗读文本不会正好等于某个选项，避免泄题。
/// 每一局都同时包含单词与句子（交替出题），关卡只决定听写拼单词的占比：1~2 关不听写，3~4 关约四分之一，5~6 关约一半。
/// </summary>
public class EnglishListeningGenerator : IPracticeItemGenerator
{
    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.EnglishListening;

    public EducationSubjectEnum Subject => EducationSubjectEnum.English;

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var band = Math.Clamp(EducationGrades.Band(grade), 0, 2);
        var words = PracticeContentBanks.EnglishWords[band];
        var sentences = PracticeContentBanks.EnglishSentences[band];
        var dictationRatio = Math.Clamp(level, 1, 6) switch
        {
            <= 2 => 0.0,
            <= 4 => 0.25,
            _ => 0.5
        };

        var items = new List<GeneratedPracticeItem>(count);
        for (var i = 0; i < count; i++)
        {
            // 奇偶交替保证一局里单词和句子都会出现，比例不会因为随机数失衡
            if (i % 2 == 1)
            {
                var picked = sentences[Random.Shared.Next(sentences.Length)];
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    "听录音，选出你听到的句子的中文意思：",
                    Shuffle(PickOthers(sentences.Select(s => s.Zh).ToList(), picked.Zh, 3).Append(picked.Zh).ToList()),
                    picked.Zh,
                    null)
                { AudioText = picked.En });

                continue;
            }

            var word = words[Random.Shared.Next(words.Length)];

            if (Random.Shared.NextDouble() < dictationRatio)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    "听录音，写出你听到的英文单词：",
                    [],
                    word.Word,
                    $"提示：一共 {word.Word.Length} 个字母")
                { AudioText = word.Word });
            }
            else
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Choice,
                    "听录音，选出你听到的单词的意思：",
                    Shuffle(PickOthers(words.Select(w => w.Meaning).ToList(), word.Meaning, 3).Append(word.Meaning).ToList()),
                    word.Meaning,
                    null)
                { AudioText = word.Word });
            }
        }

        return items;
    }

    private static List<string> PickOthers(List<string> pool, string exclude, int take) =>
        pool.Where(x => x != exclude).OrderBy(_ => Random.Shared.Next()).Distinct().Take(take).ToList();

    private static List<string> Shuffle(List<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
