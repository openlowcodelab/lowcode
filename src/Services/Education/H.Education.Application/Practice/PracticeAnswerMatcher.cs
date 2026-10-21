using System.Text;

namespace H.Education.Application.Practice;

/// <summary>
/// 练习题答案比对。
/// 选择题与拼写、挖字类按归一化后的完全一致判定（忽略大小写、空白与标点差异）；
/// 宽松判定（LooseMatch）分两种语言：
/// 英文用于对话，学生自己组织句子，只要求参考答案的关键词全部出现（缩写先展开、语气词不参与）；
/// 中文用于默写与飞花令接句，孩子习惯把整联一起写下来，作答里包含参考答案就算对。
/// </summary>
internal static class PracticeAnswerMatcher
{
    /// <summary>对话判定时忽略的高频虚词，只比关键词</summary>
    private static readonly HashSet<string> StopWords =
        new(StringComparer.Ordinal)
        {
            "a", "an", "the", "i", "you", "he", "she", "it", "we", "they", "me", "him", "us",
            "am", "is", "are", "was", "were", "be", "been", "being", "do", "does", "did",
            "have", "has", "had", "will", "would", "can", "could", "may", "might", "shall", "should",
            "yes", "no", "not", "of", "to", "in", "on", "at", "for", "and", "or", "but", "so",
            "please", "thank", "thanks", "well", "very", "my", "your", "his", "her", "our", "their",
            "this", "that", "these", "those", "there", "here", "about", "with", "from"
        };

    /// <summary>常见缩写展开，避免 "I'm" 与 "I am" 互判为错</summary>
    private static readonly (string From, string To)[] Contractions =
    [
        ("i'm", "i am"), ("it's", "it is"), ("that's", "that is"), ("what's", "what is"),
        ("there's", "there is"), ("here's", "here is"), ("he's", "he is"), ("she's", "she is"),
        ("you're", "you are"), ("we're", "we are"), ("they're", "they are"), ("i've", "i have"),
        ("you've", "you have"), ("we've", "we have"), ("they've", "they have"),
        ("i'll", "i will"), ("you'll", "you will"), ("we'll", "we will"), ("they'll", "they will"),
        ("i'd", "i would"), ("you'd", "you would"), ("we'd", "we would"), ("they'd", "they would"),
        ("let's", "let us"), ("don't", "do not"), ("doesn't", "does not"), ("didn't", "did not"),
        ("isn't", "is not"), ("aren't", "are not"), ("wasn't", "was not"), ("weren't", "were not"),
        ("can't", "can not"), ("couldn't", "could not"), ("wouldn't", "would not"),
        ("shouldn't", "should not"), ("won't", "will not"), ("haven't", "have not"), ("hasn't", "has not")
    ];

    public static bool IsCorrect(string reference, string? studentAnswer, bool looseMatch = false)
    {
        var expected = Normalize(reference);
        var actual = Normalize(studentAnswer);

        if (expected.Length == 0 || actual.Length == 0) return false;

        if (actual.Equals(expected, StringComparison.OrdinalIgnoreCase)) return true;

        var alternatives = expected.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (alternatives.Any(alt => alt.Equals(actual, StringComparison.OrdinalIgnoreCase))) return true;

        if (!looseMatch) return false;

        // 中文默写：把整联都写下来也算对，只要作答里包含参考答案
        if (HasChinese(expected))
        {
            return alternatives.Any(alt => alt.Length > 0 && actual.Contains(alt, StringComparison.OrdinalIgnoreCase));
        }

        return LooseEnglishMatches(reference, studentAnswer!);
    }

    private static bool HasChinese(string value) =>
        value.Any(c => c is >= '一' and <= '鿿');

    /// <summary>
    /// 参考答案的关键词全部出现在学生作答里即算对（允许额外成分与个别拼写偏差）。
    /// 这里必须用原始文本比对：归一化会删掉空格，"My name is Tom." 会被压成一个词而永远匹配不上。
    /// </summary>
    private static bool LooseEnglishMatches(string reference, string studentAnswer)
    {
        var actual = Keywords(studentAnswer);
        if (actual.Count == 0) return false;

        foreach (var alternative in reference.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var required = Keywords(alternative);
            if (required.Count == 0) continue;

            var missing = required.Count(word => !actual.Contains(word) && !HasNearMatch(actual, word));
            if (missing == 0) return true;

            // 句子较长时容忍一个关键词拼错
            if (required.Count >= 3 && missing == 1) return true;
        }

        return false;
    }

    private static bool HasNearMatch(IEnumerable<string> actual, string word) =>
        word.Length >= 5 && actual.Any(x => x.Length >= 5 && (x.StartsWith(word, StringComparison.Ordinal) || word.StartsWith(x, StringComparison.Ordinal)));

    private static HashSet<string> Keywords(string value)
    {
        var text = (value ?? string.Empty).ToLowerInvariant();
        foreach (var (from, to) in Contractions)
        {
            text = text.Replace(from, to, StringComparison.Ordinal);
        }

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            sb.Append(c is >= 'a' and <= 'z' ? c : ' ');
        }

        return sb.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => word.Length > 1 && !StopWords.Contains(word))
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            var normalized = c switch
            {
                >= '！' and <= '～' => (char)(c - 0xFEE0),
                '　' => ' ',
                '\r' or '\n' or '\t' => ' ',
                _ => c
            };

            if (IsNoise(normalized)) continue;
            sb.Append(normalized);
        }

        return sb.ToString().ToUpperInvariant();
    }

    private static bool IsNoise(char c) =>
        c == ' ' || c is '。' or '.' or '，' or ',' or '、' or '；' or ';' or '：' or ':'
            or '！' or '!' or '？' or '?' or '“' or '”' or '"' or '‘' or '’' or '\''
            or '「' or '」' or '『' or '』' or '《' or '》' or '〈' or '〉' or '<' or '>'
            or '(' or ')' or '（' or '）' or '[' or ']' or '{' or '}' or '·' or '—' or '-' or '_';
}
