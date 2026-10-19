using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 语文趣味·飞花令：每局随机抽令字，关卡越高玩法越硬。
/// 1~2 关四选一「找含令字的句子」，第 3 关起加反向的「找不含令字的句子」，
/// 第 4 关起按令字在句中的位置设问并要求自己默写，第 5~6 关以后两种难题为主。
/// 默写题把库里含该令字的句子全部用 | 存进答案，判定沿用「与其中任一句完全一致即对」，
/// 所以题干限定课内必背古诗：孩子写课外的句子判不出对错，只能算错。
/// </summary>
public class FeihuaLingGenerator : IPracticeItemGenerator
{
    /// <summary>低年级用的常见令字：课内诗里出现得多，孩子想得出句子</summary>
    private static readonly char[] CommonChars =
    [
        '花', '月', '风', '山', '水', '春', '日', '天', '红', '绿', '白', '云', '雨', '雪', '鸟', '草'
    ];

    /// <summary>中年级以上再放开的令字：出现得少，得更熟诗才接得上</summary>
    private static readonly char[] UpperChars =
    [
        .. CommonChars, '夜', '家', '人', '中', '江', '声', '秋', '青', '明', '楼'
    ];

    private const int MaxLevel = 6;
    private const int OptionCount = 4;

    /// <summary>答案列最长 500 字符，默写题的可接受句子按这个预算截取</summary>
    private const int AnswerBudget = 480;

    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.FeihuaLing;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Chinese;

    private record Verse(string Text, string Title, string Author)
    {
        public bool Has(char ch) => Text.Contains(ch, StringComparison.Ordinal);

        /// <summary>
        /// 令字在句中出现的下标。位置题必须按全部下标判断：
        /// 「梅子黄时日日晴」这类叠字，只要有一个下标落在指定字位就算命中。
        /// </summary>
        public int[] Occurrences(char ch) => [.. Text.Select((c, i) => c == ch ? i : -1).Where(i => i >= 0)];
    }

    private static readonly Verse[] Verses =
        [.. PracticeContentBanks.FeihuaLines.Select(x => new Verse(x.Line, x.Title, x.Author))];

    /// <summary>令字 → 含该字的句子，静态建一次索引，出题时只查不建</summary>
    private static readonly Dictionary<char, Verse[]> VersesWithChar = BuildIndex();

    private static Dictionary<char, Verse[]> BuildIndex()
    {
        var index = new Dictionary<char, List<Verse>>();

        foreach (var verse in Verses)
        {
            foreach (var ch in verse.Text.Distinct())
            {
                if (!index.TryGetValue(ch, out var list))
                {
                    list = [];
                    index[ch] = list;
                }

                list.Add(verse);
            }
        }

        return index.ToDictionary(x => x.Key, x => x.Value.ToArray());
    }

    public List<GeneratedPracticeItem> Generate(string grade, int level, int count)
    {
        var safeLevel = Math.Clamp(level, 1, MaxLevel);
        var chars = EligibleChars(grade, safeLevel);
        if (chars.Count == 0) return [];

        var used = new HashSet<string>(StringComparer.Ordinal);
        var items = new List<GeneratedPracticeItem>(count);
        List<(char Ch, Verse[] WithChar)> rotation = [];

        for (var i = 0; i < count; i++)
        {
            // 一局里尽量把可选令字轮一遍，轮完重洗，免得整局都在考同一个字
            if (rotation.Count == 0) rotation = [.. chars.OrderBy(_ => Random.Shared.Next())];
            var (ch, withChar) = rotation[0];
            rotation.RemoveAt(0);

            var item = PickVariant(safeLevel) switch
            {
                Variant.Except => ExceptItem(ch, withChar, used),
                Variant.Position => PositionItem(ch, withChar, used),
                Variant.Write => WriteItem(ch, withChar),
                _ => FindItem(ch, withChar, used)
            };

            // 难题玩法都依赖候选句够多，凑不齐就退回基础玩法
            items.Add(item ?? FindItem(ch, withChar, used));
        }

        return items;
    }

    private enum Variant
    {
        Find,
        Except,
        Position,
        Write
    }

    /// <summary>各关卡的玩法配比，数组顺序与 Variant 一致（找含令字 / 找不含 / 卡字位 / 默写）</summary>
    private static Variant PickVariant(int level)
    {
        int[] weights = level switch
        {
            <= 2 => [100, 0, 0, 0],
            3 => [55, 45, 0, 0],
            4 => [35, 25, 20, 20],
            5 => [15, 25, 30, 30],
            _ => [10, 15, 35, 40]
        };

        var roll = Random.Shared.Next(weights.Sum());
        for (var i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];
            if (roll < 0) return (Variant)i;
        }

        return Variant.Find;
    }

    /// <summary>
    /// 候选令字：关卡越高越可以考冷僻字，因为冷僻字的句子少、干扰项也少，反过来更难蒙对。
    /// 一个都不剩时退回全部候选字，宁可令字重复也不能出不了题。
    /// </summary>
    private static List<(char Ch, Verse[] WithChar)> EligibleChars(string grade, int level)
    {
        var pool = EducationGrades.Band(grade) == 0 ? CommonChars : UpperChars;
        var minVerses = level <= 2 ? 8 : level <= 4 ? 5 : 4;
        var candidates = pool
            .Select(ch => (Ch: ch, WithChar: VersesWithChar.GetValueOrDefault(ch, [])))
            .Where(x => x.WithChar.Length > 0)
            .ToList();
        List<(char Ch, Verse[] WithChar)> eligible = [.. candidates.Where(x => x.WithChar.Length >= minVerses)];

        // 一个都不剩时退回全部候选字，宁可令字重复也不能出不了题
        return eligible.Count > 0 ? eligible : candidates;
    }

    private static GeneratedPracticeItem FindItem(char ch, Verse[] withChar, HashSet<string> used)
    {
        var answer = PickVerse(withChar, used);

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Choice,
            $"飞花令「{ch}」：下面哪一句诗里有这个字？",
            Shuffle(WithoutChar(ch, answer.Title).Take(OptionCount - 1).Select(x => x.Text).Append(answer.Text)),
            answer.Text,
            Source(answer));
    }

    private static GeneratedPracticeItem? ExceptItem(char ch, Verse[] withChar, HashSet<string> used)
    {
        if (withChar.Length < OptionCount - 1) return null;

        var distract = PickDistinct(RandomOrder(withChar), used, OptionCount - 1);
        var answer = PickVerse([.. WithoutChar(ch)], used);

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Choice,
            $"飞花令「{ch}」：下面哪一句诗里没有这个字？",
            Shuffle(distract.Select(x => x.Text).Append(answer.Text)),
            answer.Text,
            $"{Source(answer)}；另外三句里都有「{ch}」");
    }

    private static GeneratedPracticeItem? PositionItem(char ch, Verse[] withChar, HashSet<string> used)
    {
        if (withChar.Length < OptionCount) return null;

        var hits = withChar.Select(x => (Verse: x, At: x.Occurrences(ch))).ToList();
        var positions = Enumerable.Range(0, hits.Max(x => x.Verse.Text.Length))
            .Where(p => hits.Any(x => x.At.Contains(p)) && hits.Count(x => !x.At.Contains(p)) >= OptionCount - 1)
            .ToList();

        if (positions.Count == 0) return null;

        var target = positions[Random.Shared.Next(positions.Count)];
        var answer = PickVerse([.. hits.Where(x => x.At.Contains(target)).Select(x => x.Verse)], used);
        var distract = PickDistinct(RandomOrder(hits.Where(x => !x.At.Contains(target)).Select(x => x.Verse)), used, OptionCount - 1);

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Choice,
            $"飞花令「{ch}」：这个字排在第 {target + 1} 个字的是哪一句？",
            Shuffle([answer.Text, .. distract.Select(x => x.Text)]),
            answer.Text,
            Source(answer));
    }

    private static GeneratedPracticeItem WriteItem(char ch, Verse[] withChar)
    {
        var accepted = new List<string>();
        var length = 0;

        foreach (var text in Shuffle(withChar.Select(x => x.Text)))
        {
            if (length + text.Length + 1 > AnswerBudget) break;
            accepted.Add(text);
            length += text.Length + 1;
        }

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Typing,
            $"飞花令「{ch}」：轮到你了，写出一句含这个字的课内必背诗句。",
            [],
            string.Join('|', accepted),
            $"含「{ch}」的课内诗句有 {withChar.Length} 句，写出任意一句都算对");
    }

    /// <summary>不含令字的句子，洗牌后当干扰项；排除与答案同出处的句子，免得出处提示把答案圈出来</summary>
    private static IEnumerable<Verse> WithoutChar(char ch, string? title = null) =>
        Verses.Where(x => !x.Has(ch) && x.Title != title).OrderBy(_ => Random.Shared.Next());
    /// <summary>随机顺序取出选项池，供 PickDistinct 按顺序补位时用</summary>
    private static List<Verse> RandomOrder(IEnumerable<Verse> pool) =>
        [.. pool.OrderBy(_ => Random.Shared.Next())];

    /// <summary>优先抽本局没出过的句子，都出过了再随机重抽</summary>
    private static Verse PickVerse(Verse[] pool, HashSet<string> used)
    {
        var fresh = pool.Where(x => !used.Contains(x.Text)).ToArray();
        var candidates = fresh.Length > 0 ? fresh : pool;
        var verse = candidates[Random.Shared.Next(candidates.Length)];
        used.Add(verse.Text);
        return verse;
    }

    /// <summary>
    /// 出处不同的句子各取一句：同首诗的两句摆在一起，找字就变成了背诗。
    /// 候选不够时先放宽出处限制，再放宽「没出过」限制补齐，所以传进来的池子要先洗牌，
    /// 否则一局做下来补位用的永远是库开头那几首。
    /// </summary>
    private static List<Verse> PickDistinct(IEnumerable<Verse> pool, HashSet<string> used, int take)
    {
        var ordered = pool.ToList();
        var picked = new List<Verse>();
        var titles = new HashSet<string>(StringComparer.Ordinal);

        foreach (var verse in ordered)
        {
            if (used.Contains(verse.Text) || titles.Contains(verse.Title)) continue;
            picked.Add(verse);
            titles.Add(verse.Title);
            if (picked.Count == take) return picked;
        }

        foreach (var verse in ordered)
        {
            if (picked.Any(x => x.Text == verse.Text)) continue;
            picked.Add(verse);
            if (picked.Count == take) break;
        }

        return picked;
    }

    private static string Source(Verse verse) => $"{verse.Author}《{verse.Title}》";

    private static List<string> Shuffle(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
