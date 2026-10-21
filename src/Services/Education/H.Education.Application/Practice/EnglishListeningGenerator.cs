using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 英语听力练习出题器，按模式分三条路：
/// 单词——听词选中文意思，第 3 关起掺入听写拼单词；
/// 句子——听句选中文意思，第 4 关起掺入听写整句（按关键词宽松判定，标点与个别拼写不苛求）；
/// 短文——朗读一整段，按段意选答案，第 4 关起掺入「补全短文中缺的单词」。
/// 音频由浏览器 Web Speech 合成，服务端只下发要朗读的英文原文；
/// 选择题一律用中文给选项，朗读文本不会正好等于某个选项，避免泄题。
/// </summary>
public class EnglishListeningGenerator : IPracticeItemGenerator
{
    public const string ModeWord = "word";
    public const string ModeSentence = "sentence";
    public const string ModePassage = "passage";

    private const int MaxLevel = 6;

    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.EnglishListening;

    public EducationSubjectEnum Subject => EducationSubjectEnum.English;

    public List<GeneratedPracticeItem> Generate(PracticeGenerateRequest request)
    {
        var band = Math.Clamp(EducationGrades.Band(request.Grade), 0, 2);
        var level = Math.Clamp(request.Level, 1, MaxLevel);

        return request.Mode switch
        {
            ModeSentence => Sentences(band, level, request.Count),
            ModePassage => Passages(band, level, request.Count),
            _ => Words(band, level, request.Count)
        };
    }

    private static List<GeneratedPracticeItem> Words(int band, int level, int count)
    {
        var bank = PracticeContentBanks.EnglishWords[band];
        // 听写拼单词只在第 3 关以后出现，且最多一半，不然孩子全程在写字没几个在听
        var dictationRatio = level <= 2 ? 0.0 : Math.Min(0.5, 0.12 * level);

        var items = new List<GeneratedPracticeItem>(count);
        foreach (var word in Draw(bank, count))
        {
            if (Random.Shared.NextDouble() < dictationRatio)
            {
                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    "听录音，写出你听到的英文单词：",
                    [],
                    word.Word,
                    $"一共 {word.Word.Length} 个字母")
                { AudioText = word.Word });

                continue;
            }

            items.Add(new GeneratedPracticeItem(
                PracticeInputTypeEnum.Choice,
                "听录音，选出你听到的单词的意思：",
                Shuffle(PickOthers(bank.Select(w => w.Meaning), word.Meaning, 3).Append(word.Meaning)),
                word.Meaning,
                null)
            { AudioText = word.Word });
        }

        return items;
    }

    private static List<GeneratedPracticeItem> Sentences(int band, int level, int count)
    {
        var bank = PracticeContentBanks.EnglishSentences[band];
        var dictationRatio = level <= 3 ? 0.0 : Math.Min(0.5, 0.15 * (level - 2));

        var items = new List<GeneratedPracticeItem>(count);
        foreach (var sentence in Draw(bank, count))
        {
            if (Random.Shared.NextDouble() < dictationRatio)
            {
                var words = sentence.En.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

                items.Add(new GeneratedPracticeItem(
                    PracticeInputTypeEnum.Typing,
                    "听录音，写出你听到的句子：",
                    [],
                    sentence.En,
                    level <= 4 ? $"中文：{sentence.Zh} · 共 {words} 个词" : $"一共 {words} 个词")
                { AudioText = sentence.En, LooseMatch = true });

                continue;
            }

            items.Add(new GeneratedPracticeItem(
                PracticeInputTypeEnum.Choice,
                "听录音，选出你听到的句子的中文意思：",
                Shuffle(PickOthers(bank.Select(x => x.Zh), sentence.Zh, 3).Append(sentence.Zh)),
                sentence.Zh,
                null)
            { AudioText = sentence.En });
        }

        return items;
    }

    /// <summary>
    /// 短文模式按篇出题：同一段的几道理解题连着出现，孩子听完一遍就能连着答几题。
    /// 题量超过一篇的容量就换下一篇，全用完再重洗一轮。
    /// </summary>
    private static List<GeneratedPracticeItem> Passages(int band, int level, int count)
    {
        var bank = PracticeContentBanks.EnglishPassages[band];
        var deck = new List<GeneratedPracticeItem>();

        for (var round = 0; round < 4 && deck.Count < count; round++)
        {
            foreach (var passage in bank.OrderBy(_ => Random.Shared.Next()))
            {
                foreach (var question in passage.Questions)
                {
                    deck.Add(QuestionItem(passage.Text, question));
                }

                if (level >= 4)
                {
                    foreach (var cloze in passage.Clozes)
                    {
                        deck.Add(ClozeItem(passage.Text, cloze));
                    }
                }

                if (deck.Count >= count) break;
            }
        }

        return [.. deck.Take(count)];
    }

    private static GeneratedPracticeItem QuestionItem(
        string audio,
        PracticeContentBanks.PassageQuestion question) => new(
            PracticeInputTypeEnum.Choice,
            $"听短文，选出正确答案：\n{question.Ask}",
            Shuffle(question.Options),
            question.Answer,
            null)
        { AudioText = audio };

    private static GeneratedPracticeItem ClozeItem(
        string audio,
        PracticeContentBanks.PassageCloze cloze) => new(
            PracticeInputTypeEnum.Typing,
            $"听短文，补全句中缺少的单词：\n{cloze.Cloze}",
            [],
            cloze.Word,
            $"缺的词意思是「{cloze.Meaning}」，共 {cloze.Word.Length} 个字母")
        { AudioText = audio };

    /// <summary>一轮里不重复抽题，题库不够就再洗一轮补满，保证题量足且不会连着出同一条</summary>
    private static List<T> Draw<T>(T[] bank, int count)
    {
        var picked = new List<T>(count);
        if (bank.Length == 0) return picked;

        while (picked.Count < count)
        {
            foreach (var item in bank.OrderBy(_ => Random.Shared.Next()))
            {
                picked.Add(item);
                if (picked.Count == count) break;
            }
        }

        return picked;
    }

    private static List<string> PickOthers(IEnumerable<string> pool, string exclude, int take) =>
        pool.Where(x => x != exclude).OrderBy(_ => Random.Shared.Next()).Distinct().Take(take).ToList();

    private static List<string> Shuffle(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
