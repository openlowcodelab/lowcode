using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using H.AI.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.EntityFrameworkCore;

namespace H.Education.Application.Grading;

/// <summary>
/// 单题批改结果
/// </summary>
public record GradeOutcome(bool? IsCorrect, decimal AwardedScore, JudgeModeEnum JudgeMode, string? Feedback);

/// <summary>
/// 自动评分器。
/// 客观题（单选、多选、判断、填空、计算）在服务端按规则精确判定，不依赖 AI；
/// 主观题（简答、作文）优先让 AI 按评分要点给分，AI 不可用时退回「参考答案要点命中 + 篇幅」的启发式评分，
/// 并在结果的判定方式里如实标注是哪一种，避免把兜底分数当成 AI 分数。
/// </summary>
public class AnswerGrader
{
    private readonly IAiCompletionAppService _aiCompletionAppService;

    public AnswerGrader(IAiCompletionAppService aiCompletionAppService)
    {
        _aiCompletionAppService = aiCompletionAppService;
    }

    public async Task<GradeOutcome> GradeAsync(ExamQuestionEntity question, string? studentAnswer)
    {
        var type = (QuestionTypeEnum)question.Type;
        var raw = studentAnswer?.Trim() ?? string.Empty;
        var normalized = Normalize(raw);

        if (normalized.Length == 0)
        {
            return new GradeOutcome(false, 0, JudgeModeEnum.None, "未作答");
        }

        if (type.IsObjective())
        {
            var correct = IsObjectiveCorrect(type, question.Answer, normalized);
            return new GradeOutcome(correct, correct ? question.Score : 0, JudgeModeEnum.Auto, null);
        }

        // 主观题把原始作答交给模型，保留空格与标点，英文句子评分才准确
        return await GradeSubjectiveAsync(type, question, raw);
    }

    private static bool IsObjectiveCorrect(QuestionTypeEnum type, string reference, string studentAnswer) => type switch
    {
        QuestionTypeEnum.SingleChoice => Equals(FirstLetter(reference), FirstLetter(studentAnswer)),
        QuestionTypeEnum.MultipleChoice => Equals(LetterSet(reference), LetterSet(studentAnswer)),
        QuestionTypeEnum.TrueFalse => Equals(TruthText(reference), TruthText(studentAnswer)),
        QuestionTypeEnum.FillBlank => FillBlankMatches(reference, studentAnswer),
        QuestionTypeEnum.Calculation => CalculationMatches(reference, studentAnswer),
        _ => false
    };

    /// <summary>
    /// 填空题：多空按「；」分隔逐空比较，同一空的多个可接受答案用「|」分隔。
    /// 学生常用逗号或直接把答案写成一句话，因此先按分隔符逐空比对，再退化为包含判断。
    /// </summary>
    private static bool FillBlankMatches(string reference, string studentAnswer)
    {
        var studentTokens = SplitTokens(studentAnswer);

        foreach (var slot in SplitSlots(reference))
        {
            var acceptable = slot.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (acceptable.Length == 0) continue;

            var matched = acceptable.Any(a => studentTokens.Any(s => EqualsToken(a, s)))
                          || acceptable.Any(a => studentAnswer.Contains(Normalize(a), StringComparison.OrdinalIgnoreCase));
            if (!matched) return false;
        }

        return true;
    }

    /// <summary>
    /// 计算题：参考答案里的每个得数都要在学生作答中出现（允许 0.0001 误差与「1/4」这类分数写法），
    /// 学生写了解题过程会带出中间数值，因此按子集匹配而不是按数值个数比较
    /// </summary>
    private static bool CalculationMatches(string reference, string studentAnswer)
    {
        var referenceValues = Numbers(reference);
        var studentValues = Numbers(studentAnswer);

        if (referenceValues.Count > 0 && studentValues.Count > 0)
        {
            return referenceValues.All(r => studentValues.Any(s => Math.Abs(r - s) < 0.0001));
        }

        return reference.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Any(a => EqualsToken(a, studentAnswer));
    }

    private static List<string> SplitSlots(string value) =>
        value.Split(['；', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static List<string> SplitTokens(string value) =>
        value.Split(['；', ';', '，', ',', '、', '。', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    private static bool EqualsToken(string left, string right) =>
        string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var text = value.Trim();
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            var normalized = c switch
            {
                >= '！' and <= '～' => (char)(c - 0xFEE0), // 全角转半角
                '　' => ' ',
                '\r' or '\n' or '\t' => ' ',
                _ => c
            };
            if (normalized != ' ') sb.Append(normalized);
        }

        text = sb.ToString();
        text = Regex.Replace(text, @"^(答案|答|结果|得数|解)[:：]?", string.Empty);
        text = text.TrimEnd('。', '.', '！', '!', '？', '?', '；', ';');
        return text.ToUpperInvariant();
    }

    private static string FirstLetter(string value)
    {
        var match = Regex.Match(value, @"[A-Da-d]");
        return match.Success ? match.Value.ToUpperInvariant() : string.Empty;
    }

    private static string LetterSet(string value)
    {
        var letters = Regex.Matches(value, @"[A-Da-d]")
            .Select(m => m.Value.ToUpperInvariant())
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal);
        return string.Concat(letters);
    }

    private static string TruthText(string value)
    {
        var text = Normalize(value);
        if (text.Length == 0) return string.Empty;

        // 先匹配「不对/错误」这类含否定词的说法，避免被首字符误判为肯定
        if (text.Contains("错") || text.Contains("误") || text.Contains("不对") || text.Contains("×") || text.Contains("X"))
            return "错";
        if (text.Contains("对") || text.Contains("正") || text.Contains("√") || text is "T" or "TRUE" or "YES" or "是")
            return "对";
        return text;
    }

    private static List<double> Numbers(string value)
    {
        var result = new List<double>();
        foreach (Match match in Regex.Matches(value, @"-?\d+(?:\.\d+)?(?:\s*/\s*-?\d+(?:\.\d+)?)?"))
        {
            var token = match.Value.Replace(" ", string.Empty);
            if (token.Contains('/'))
            {
                var parts = token.Split('/');
                if (parts.Length == 2 && double.TryParse(parts[0], out var numerator) &&
                    double.TryParse(parts[1], out var denominator) && denominator != 0)
                {
                    result.Add(numerator / denominator);
                }
                continue;
            }

            if (double.TryParse(token, out var number)) result.Add(number);
        }

        return result;
    }

    private async Task<GradeOutcome> GradeSubjectiveAsync(QuestionTypeEnum type, ExamQuestionEntity question, string studentAnswer)
    {
        try
        {
            var response = await _aiCompletionAppService.CompleteAsync(new AiCompletionInputDto
            {
                SystemPrompt = "你是一名严格按评分要点给分的老师。只输出一个 JSON 对象，不要输出其它文字。",
                UserMessage = BuildScorePrompt(type, question, studentAnswer),
                Temperature = 0.1f,
                // 思考型模型的思维链也算进 max_tokens，给小了会解析不到分数而退回要点估算
                MaxTokens = 2000
            });

            var outcome = ParseAiScore(response.Data?.Content, question.Score);
            if (outcome is not null) return outcome;
        }
        catch (Exception)
        {
            // AI 批改不可用的原因很多（未配置默认模型、限流、返回不是 JSON），
            // 交卷不能因此整体失败，统一退回启发式评分并在逐题评语里说明
        }

        return GradeByKeywords(question, studentAnswer);
    }

    private static string BuildScorePrompt(QuestionTypeEnum type, ExamQuestionEntity question, string studentAnswer)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"题型：{type.GetQuestionTypeName()}，本题满分 {question.Score} 分。");
        sb.AppendLine($"题目：{question.Content}");
        sb.AppendLine($"参考答案：{question.Answer}");
        if (!string.IsNullOrWhiteSpace(question.Analysis))
        {
            sb.AppendLine($"评分要点：{question.Analysis}");
        }
        sb.AppendLine($"学生作答：{studentAnswer}");
        sb.AppendLine();
        sb.AppendLine("""
            按下面的 JSON 结构输出：
            { "score": 得分数字, "comment": "一句话点评，指出得分或扣分原因" }
            得分必须是 0 到本题满分之间的数字，允许 0.5 分。学生作答只要意思与参考答案一致就应给分，不要因表述不同而扣分。
            """);
        return sb.ToString();
    }

    private static GradeOutcome? ParseAiScore(string? content, decimal fullScore)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            using var document = JsonDocument.Parse(content[start..(end + 1)]);
            var root = document.RootElement;

            if (!root.TryGetProperty("score", out var scoreElement)) return null;

            var score = scoreElement.ValueKind switch
            {
                JsonValueKind.Number => (decimal)scoreElement.GetDouble(),
                JsonValueKind.String => decimal.TryParse(scoreElement.GetString(), out var parsed) ? parsed : (decimal?)null,
                _ => null
            };
            if (score is null) return null;

            var comment = root.TryGetProperty("comment", out var commentElement)
                ? commentElement.GetString()
                : null;

            var awarded = Math.Clamp(Math.Round(score.Value * 2, MidpointRounding.AwayFromZero) / 2, 0, fullScore);
            return new GradeOutcome(awarded >= fullScore * 0.6m, awarded, JudgeModeEnum.Ai, comment);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// AI 不可用时的兜底评分。
    /// 先认与参考答案一致的作答（孩子写得比参考简短或多了标点都应得满分），
    /// 再退回二元字组覆盖率 + 篇幅，字组只跟参考答案比，不拿「评分要点」那种长句去比。
    /// </summary>
    private static GradeOutcome GradeByKeywords(ExamQuestionEntity question, string studentAnswer)
    {
        var normalizedStudent = Normalize(studentAnswer);
        var alternatives = Normalize(question.Answer)
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var alternative in alternatives)
        {
            if (normalizedStudent.Equals(alternative, StringComparison.OrdinalIgnoreCase) ||
                normalizedStudent.Contains(alternative, StringComparison.OrdinalIgnoreCase))
            {
                return new GradeOutcome(true, question.Score, JudgeModeEnum.Keyword,
                    "作答与参考答案一致，已按参考答案给满分（AI 批改不可用）");
            }
        }

        var referenceBigrams = Bigrams(string.Join(string.Empty, alternatives));
        var studentBigrams = Bigrams(normalizedStudent);

        var coverage = referenceBigrams.Count == 0
            ? 0.5
            : (double)referenceBigrams.Count(studentBigrams.Contains) / referenceBigrams.Count;

        var expectedLength = question.Type == (int)QuestionTypeEnum.Composition ? 300 : 40;
        var lengthRatio = Math.Min(1.0, studentAnswer.Trim().Length / (double)expectedLength);

        var raw = (double)question.Score * (0.7 * coverage + 0.3 * lengthRatio);
        var awarded = Math.Clamp(Math.Round((decimal)raw * 2, MidpointRounding.AwayFromZero) / 2, 0, question.Score);

        var feedback = $"AI 批改不可用，按参考答案要点与篇幅估算得分（要点覆盖率 {(coverage * 100):F0}%，作答 {studentAnswer.Trim().Length} 字）";
        return new GradeOutcome(awarded >= question.Score * 0.6m, awarded, JudgeModeEnum.Keyword, feedback);
    }

    private static List<string> Bigrams(string value)
    {
        var result = new List<string>();
        for (var i = 0; i + 1 < value.Length; i++)
        {
            result.Add(value.Substring(i, 2));
        }

        return result.Distinct(StringComparer.Ordinal).ToList();
    }
}
