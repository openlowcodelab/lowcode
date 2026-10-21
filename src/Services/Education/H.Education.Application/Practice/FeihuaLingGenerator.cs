using System.Text;
using H.Education.Application.Contracts;
using Volo.Abp;

namespace H.Education.Application.Practice;

/// <summary>
/// 语文趣味·飞花令：令字由学生自己输入（一个字或一个词，如「月」「春风」），
/// 出题只围绕课内必背诗里含这个词的句子，一律用填空作答：
/// 「相邻句填空」给同诗的上一句或下一句，默写出含令字的那句；
/// 「句中挖字」把含令字的一小段挖掉补回来；
/// 「自己接句」不给提示，自己写一句含令字的诗，撞上库里任意一句都算对。
/// 关卡越高越靠后两种，第 5 关起以句中挖字为主、自由接句一局只出两三次。
/// </summary>
public class FeihuaLingGenerator : IPracticeItemGenerator
{
    private const int MaxLevel = 6;

    /// <summary>答案列最长 500 字符，自由接句的可接受句子按这个预算截取</summary>
    private const int AnswerBudget = 480;

    /// <summary>令字最长几个字，超出截断，免得把一整句诗贴进来当关键词</summary>
    private const int MaxKeywordLength = 8;

    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.FeihuaLing;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Chinese;

    /// <summary>诗句 + 它在同一首诗里的相邻句，相邻句用来出「给上句写下句」这类填空</summary>
    private record Verse(string Text, string Title, string Author, string? Prev, string? Next);

    private static readonly Verse[] Verses = BuildVerses();

    private enum Variant
    {
        Pair,
        Mask,
        Free
    }

    public List<GeneratedPracticeItem> Generate(PracticeGenerateRequest request)
    {
        var keyword = NormalizeKeyword(request.Keyword);
        if (keyword.Length == 0)
        {
            throw new UserFriendlyException("请先输入一个关键字或词，例如「月」或「春风」。");
        }

        var level = Math.Clamp(request.Level, 1, MaxLevel);
        var matches = Verses.Where(v => v.Text.Contains(keyword, StringComparison.Ordinal)).ToArray();
        if (matches.Length == 0)
        {
            throw new UserFriendlyException($"课内必背古诗里没有含「{keyword}」的句子，换一个常见的字或词再试。");
        }

        var items = new List<GeneratedPracticeItem>(request.Count);
        List<Verse> rotation = [];
        // 自由接句整局题干一模一样，出两三次就够了，多了等于让孩子反复写同一句话
        var freeLeft = Math.Max(1, (int)Math.Ceiling(request.Count * 0.25));

        for (var i = 0; i < request.Count; i++)
        {
            // 含这个词的句子先轮一遍再重洗，一局里尽量把课内相关的句子都过掉
            if (rotation.Count == 0) rotation = [.. matches.OrderBy(_ => Random.Shared.Next())];
            var verse = rotation[0];
            rotation.RemoveAt(0);

            var variant = PickVariant(level);
            if (variant == Variant.Free && freeLeft == 0) variant = Variant.Pair;

            var item = variant switch
            {
                // 多字令词出不了挖字题，退回相邻句填空，别一律掉到自由接句
                Variant.Mask => MaskItem(keyword, verse, level) ?? PairItem(keyword, verse, level),
                Variant.Free => FreeItem(keyword, matches, level),
                _ => PairItem(keyword, verse, level) ?? MaskItem(keyword, verse, level)
            };

            if (variant == Variant.Free) freeLeft--;
            // 凑不出填空就退回自由接句，自由接句永远有题
            items.Add(item ?? FreeItem(keyword, matches, level));
        }

        return items;
    }

    /// <summary>去掉空白与标点，只留汉字和字母；超过上限截断</summary>
    private static string NormalizeKeyword(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return string.Empty;

        var builder = new StringBuilder(keyword.Length);

        foreach (var ch in keyword.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch is >= '一' and <= '鿿') builder.Append(ch);
            if (builder.Length == MaxKeywordLength) break;
        }

        return builder.ToString();
    }

    /// <summary>
    /// 各关卡的玩法配比，数组顺序与 Variant 一致（相邻句填空 / 句中挖字 / 自由接句）。
    /// 自由接句的题干整局都一样，实际上限由 Generate 里的 freeLeft 卡住。
    /// </summary>
    private static Variant PickVariant(int level)
    {
        int[] weights = level switch
        {
            1 => [100, 0, 0],
            2 => [70, 30, 0],
            3 => [50, 25, 25],
            4 => [35, 35, 30],
            5 => [20, 50, 30],
            _ => [10, 60, 30]
        };

        var roll = Random.Shared.Next(weights.Sum());
        for (var i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];
            if (roll < 0) return (Variant)i;
        }

        return Variant.Pair;
    }

    private static GeneratedPracticeItem? PairItem(string keyword, Verse verse, int level)
    {
        var stem = verse.Prev is not null ? $"{verse.Prev}，____"
            : verse.Next is not null ? $"____，{verse.Next}"
            : null;

        if (stem is null) return null;

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Typing,
            $"飞花令「{keyword}」：补全含「{keyword}」的那句诗\n{stem}",
            [],
            verse.Text,
            level <= 3 ? $"出自{verse.Author}《{verse.Title}》" : null)
        { LooseMatch = true };
    }

    /// <summary>
    /// 把含令字的一小段挖掉补回来；答案同时收下「只填缺的字」与「整句写全」两种写法。
    /// 只有单字令口才出这种题：多字令词挖出来必然连词带字（「春风又」），
    /// 反而是在问「春风又后面是什么」，那种题交给相邻句填空更合适。
    /// </summary>
    private static GeneratedPracticeItem? MaskItem(string keyword, Verse verse, int level)
    {
        if (keyword.Length > 1) return null;

        var clauses = verse.Text
            .Split('，', StringSplitOptions.RemoveEmptyEntries)
            .Select(text => (Text: text, Hits: IndicesOf(text, keyword)))
            .Where(x => x.Hits.Length > 0)
            .ToList();

        if (clauses.Count == 0) return null;

        var chosen = clauses[Random.Shared.Next(clauses.Count)];
        var at = chosen.Hits[Random.Shared.Next(chosen.Hits.Length)];
        var start = MaskWindow(chosen.Text, at);
        if (start < 0) return null;

        var missing = chosen.Text.Substring(start, 2);
        var masked = string.Concat(chosen.Text[..start], "＿＿", chosen.Text[(start + 2)..]);

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Typing,
            $"飞花令「{keyword}」：补全这句里缺的字\n{masked}",
            [],
            $"{missing}|{chosen.Text}",
            level <= 2 ? $"含「{keyword}」，出自{verse.Author}《{verse.Title}》" : $"这一句含「{keyword}」");
    }

    /// <summary>
    /// 挖连续两个字，既盖住令字、起点又尽量落在两字一节的节奏上：
    /// 「秦时明月汉时关」里的「月」挖出来是「明月」，不是「月汉」这种不成词的片段。
    /// 实在找不到偶数起点时退回奇数起点，再不行就不出这道题。
    /// </summary>
    private static int MaskWindow(string clause, int at)
    {
        if (clause.Length < 3) return -1;

        var low = Math.Max(0, at - 1);
        var high = Math.Min(at, clause.Length - 2);
        if (high < low) return -1;

        var aligned = new List<int>();
        var fallback = new List<int>();
        for (var start = low; start <= high; start++)
        {
            (start % 2 == 0 ? aligned : fallback).Add(start);
        }

        var pool = aligned.Count > 0 ? aligned : fallback;
        return pool.Count == 0 ? -1 : pool[Random.Shared.Next(pool.Count)];
    }

    /// <summary>自由接句：库里所有含令字的句子都算对，用 | 存进答案交给宽松判定</summary>
    private static GeneratedPracticeItem FreeItem(string keyword, Verse[] matches, int level)
    {
        var accepted = new List<string>();
        var length = 0;

        foreach (var text in matches.Select(x => x.Text).Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()))
        {
            if (length + text.Length + 1 > AnswerBudget) break;
            accepted.Add(text);
            length += text.Length + 1;
        }

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Typing,
            $"飞花令「{keyword}」：轮到你了，写出一句含「{keyword}」的课内诗句",
            [],
            string.Join('|', accepted),
            level <= 4 ? $"课内含「{keyword}」的诗句有 {matches.Length} 句，写出任意一句都算对" : null)
        { LooseMatch = true };
    }

    private static int[] IndicesOf(string text, string keyword)
    {
        var hits = new List<int>();
        for (var i = text.IndexOf(keyword, StringComparison.Ordinal); i >= 0; i = text.IndexOf(keyword, i + 1, StringComparison.Ordinal))
        {
            hits.Add(i);
        }

        return [.. hits];
    }

    /// <summary>内容库是按诗连续录入的，按标题切组就能拿到每句的相邻句</summary>
    private static Verse[] BuildVerses()
    {
        var result = new List<Verse>();
        var group = new List<(string Line, string Title, string Author)>();

        void Flush()
        {
            for (var i = 0; i < group.Count; i++)
            {
                var entry = group[i];
                result.Add(new Verse(entry.Line, entry.Title, entry.Author,
                    i > 0 ? group[i - 1].Line : null,
                    i + 1 < group.Count ? group[i + 1].Line : null));
            }

            group.Clear();
        }

        foreach (var entry in PracticeContentBanks.FeihuaLines)
        {
            if (group.Count > 0 && entry.Title != group[^1].Title) Flush();
            group.Add(entry);
        }

        Flush();
        return [.. result];
    }
}
