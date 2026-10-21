using H.Education.Application.Contracts;

namespace H.Education.Application.Practice;

/// <summary>
/// 语文诗词填空出题器。
/// 选题池先按年级分档（三年级就考三、四年级的诗，不再一上来是「床前明月光」），
/// 再由关卡决定题型深浅：第 1 关认读（选上句/选下句），第 2 关起挖字补回，第 3 关起整句默写，
/// 第 4 关起辨出处，第 5 关起撤掉出处提示并要求默写上一句，第 6 关以默写与挖字为主。
/// </summary>
public class ChinesePoemGenerator : IPracticeItemGenerator
{
    private const int MaxLevel = 6;
    private const int OptionCount = 4;

    /// <summary>一局最多 15 题，本学段不足 15 首时才向邻档借诗，保证尽量不重题</summary>
    private const int MinPool = 15;

    private record Poem(string Title, string Author, string First, string Second, int Grade);

    private static readonly Poem[] Bank =
        [.. PracticeContentBanks.Poems.Select(x => new Poem(x.Title, x.Author, x.First, x.Second, x.Grade))];

    public PracticeGameTypeEnum GameType => PracticeGameTypeEnum.ChinesePoem;

    public EducationSubjectEnum Subject => EducationSubjectEnum.Chinese;

    private enum Variant
    {
        ChoiceNext,
        ChoicePrev,
        ChoiceSource,
        TypeNext,
        TypePrev,
        MaskLine
    }

    public List<GeneratedPracticeItem> Generate(PracticeGenerateRequest request)
    {
        var level = Math.Clamp(request.Level, 1, MaxLevel);
        var pool = PickPool(request.Grade);

        var items = new List<GeneratedPracticeItem>(request.Count);
        List<Poem> rotation = [];

        for (var i = 0; i < request.Count; i++)
        {
            // 一局里先把本学段的诗轮一遍再重洗，免得连着问同一首
            if (rotation.Count == 0) rotation = [.. pool.OrderBy(_ => Random.Shared.Next())];
            var poem = rotation[0];
            rotation.RemoveAt(0);

            var others = PickDistractors(pool, poem);
            // 认读题永远出得出来，用它兜住难题题型凑不出题的情况
            items.Add(Build(PickVariant(level), poem, others, level) ?? ChoiceLine(poem, others, second: true));
        }

        return items;
    }

    /// <summary>年级 → 取诗的年级档；认不出年级时按小学中年级出，宁难不难</summary>
    private static int[] PreferredGrades(string grade) => EducationGrades.Parse(grade) switch
    {
        0 => [3, 4],
        <= 2 => [1, 2],
        <= 4 => [3, 4],
        <= 6 => [5, 6],
        <= 9 => [7, 8, 9],
        _ => [10, 9]
    };

    /// <summary>本学段够出一局就只用本学段，不够时按与本轮档中心最近的年级借诗（高中只有几首，必须借初中）</summary>
    private static List<Poem> PickPool(string grade)
    {
        var wanted = PreferredGrades(grade);
        var pool = Bank.Where(p => wanted.Contains(p.Grade)).ToList();
        if (pool.Count >= MinPool) return pool;

        var center = wanted.Average();
        pool.AddRange(Bank
            .Where(p => !wanted.Contains(p.Grade))
            .OrderBy(p => Math.Abs(p.Grade - center))
            .ThenBy(_ => Random.Shared.Next())
            .Take(MinPool - pool.Count));

        return pool;
    }

    /// <summary>各关卡的题型配比，数组顺序与 Variant 一致（选下句/选上句/辨出处/默写下句/默写上句/挖字）</summary>
    private static Variant PickVariant(int level)
    {
        int[] weights = level switch
        {
            1 => [60, 40, 0, 0, 0, 0],
            2 => [40, 25, 0, 0, 0, 35],
            3 => [25, 15, 0, 30, 0, 30],
            4 => [18, 10, 20, 27, 10, 15],
            5 => [10, 8, 15, 22, 25, 20],
            _ => [5, 5, 12, 18, 30, 30]
        };

        var roll = Random.Shared.Next(weights.Sum());
        for (var i = 0; i < weights.Length; i++)
        {
            roll -= weights[i];
            if (roll < 0) return (Variant)i;
        }

        return Variant.ChoiceNext;
    }

    private static GeneratedPracticeItem? Build(Variant variant, Poem poem, List<Poem> others, int level) => variant switch
    {
        Variant.ChoicePrev => ChoiceLine(poem, others, second: false),
        Variant.ChoiceSource when others.Count > 0 => ChoiceSource(poem, others),
        Variant.TypeNext => TypeLine(poem, level, second: true),
        Variant.TypePrev => TypeLine(poem, level, second: false),
        Variant.MaskLine => MaskLine(poem, level),
        _ => ChoiceLine(poem, others, second: true)
    };

    /// <summary>给上句选下句（second=true），或给下句选上句</summary>
    private static GeneratedPracticeItem ChoiceLine(Poem poem, List<Poem> others, bool second)
    {
        var answer = second ? poem.Second : poem.First;

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Choice,
            second ? $"补全诗句：{poem.First}，____" : $"补全诗句：____，{poem.Second}",
            Shuffle(others.Select(x => second ? x.Second : x.First).Append(answer)),
            answer,
            $"出自{poem.Author}《{poem.Title}》");
    }

    /// <summary>
    /// 整句默写。孩子常把一联一起写下来，所以按宽松判定：
    /// 默写下句时写「欲穷千里目，更上一层楼」也算对。
    /// </summary>
    private static GeneratedPracticeItem TypeLine(Poem poem, int level, bool second) => new(
        PracticeInputTypeEnum.Typing,
        second ? $"默写下一句：{poem.First}，____" : $"默写上一句：____，{poem.Second}",
        [],
        second ? poem.Second : poem.First,
        // 第 5 关起不再告诉出处，能不能背下来全看自己
        level <= 4 ? $"出自{poem.Author}《{poem.Title}》" : null)
    { LooseMatch = true };

    private static GeneratedPracticeItem? ChoiceSource(Poem poem, List<Poem> others)
    {
        if (others.Count == 0) return null;

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Choice,
            $"「{poem.First}，{poem.Second}」出自哪首古诗？",
            Shuffle(others.Select(x => x.Title).Append(poem.Title)),
            poem.Title,
            poem.Author);
    }

    /// <summary>
    /// 挖掉小句末尾的一截让孩子补回来（「莫愁前路＿＿＿」→「无知己」）。
    /// 从中间挖会把「无知己」切成「无知」这种不像词的片段，判分与讲解都难看。
    /// 按逗号切成小句再挖，否则「天苍苍，野茫茫」这种跨句的诗会把标点一起挖进空里。
    /// 答案同时收下「只填缺的字」和「把整小句写全」两种写法。
    /// </summary>
    private static GeneratedPracticeItem? MaskLine(Poem poem, int level)
    {
        var clauses = (level % 2 == 0 ? poem.Second : poem.First)
            .Split('，', StringSplitOptions.RemoveEmptyEntries);
        if (clauses.Length == 0) return null;

        var clause = clauses[Random.Shared.Next(clauses.Length)];
        if (clause.Length < 4) return null;

        // 至少留一个字认句，也至少挖掉两个字：只挖一个字等于送分
        var start = Random.Shared.Next(1, clause.Length - 2);
        var missing = clause[start..];
        var masked = string.Concat(clause[..start], new string('＿', missing.Length));

        return new GeneratedPracticeItem(
            PracticeInputTypeEnum.Typing,
            $"补全诗句中缺少的字：{masked}",
            [],
            $"{missing}|{clause}",
            level <= 3 ? $"出自{poem.Author}《{poem.Title}》" : null);
    }

    /// <summary>
    /// 干扰项优先从同一年级的池子里取，让孩子看到的四个选项都是学过的句子；
    /// 池子太小再向整库补，且按出处去重，同首诗的两句摆在一起就没法猜了。
    /// 两批各自洗牌后拼接，先全局洗牌会把「同年级优先」冲掉。
    /// </summary>
    private static List<Poem> PickDistractors(List<Poem> pool, Poem poem)
    {
        var titles = new HashSet<string>(StringComparer.Ordinal) { poem.Title };
        var sameGrade = pool.Where(x => x.Title != poem.Title).OrderBy(_ => Random.Shared.Next());
        var otherGrade = Bank
            .Where(x => x.Title != poem.Title && !pool.Contains(x))
            .OrderBy(_ => Random.Shared.Next());

        var picked = new List<Poem>(OptionCount - 1);
        foreach (var candidate in sameGrade.Concat(otherGrade))
        {
            if (picked.Count == OptionCount - 1) break;
            if (titles.Add(candidate.Title)) picked.Add(candidate);
        }

        return picked;
    }

    private static List<string> Shuffle(IEnumerable<string> values) =>
        values.Distinct(StringComparer.Ordinal).OrderBy(_ => Random.Shared.Next()).ToList();
}
