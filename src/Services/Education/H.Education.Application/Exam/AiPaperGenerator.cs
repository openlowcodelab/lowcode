using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using H.AI.Application.Contracts;
using H.Education.Application.Contracts;
using H.Util.Base;
using Volo.Abp;

namespace H.Education.Application.Exam;

/// <summary>
/// AI 生成的试卷（尚未落库的中间结果）
/// </summary>
public record GeneratedQuestion(
    string? Section,
    QuestionTypeEnum Type,
    string Content,
    List<QuestionOptionDto> Options,
    string Answer,
    string? Analysis,
    decimal Score);

/// <summary>
/// AI 生成的试卷
/// </summary>
public record GeneratedPaper(
    string Title,
    int DurationMinutes,
    List<GeneratedQuestion> Questions,
    string Prompt,
    string? Model);

/// <summary>
/// AI 试卷命题器：把命题要求转成提示词交给 AI 应用的默认模型，再把返回的 JSON 解析为试题。
/// 模型输出的键名与结构并不总是听话（选项写成字符串、answer 直接给选项原文、最外层是数组等），
/// 所以解析按「常见变体都兼容」来写；分值与题量以解析结果为准，最后重排大题并按实际分值累加总分。
/// </summary>
public class AiPaperGenerator
{
    private readonly IAiCompletionAppService _aiCompletionAppService;

    public AiPaperGenerator(IAiCompletionAppService aiCompletionAppService)
    {
        _aiCompletionAppService = aiCompletionAppService;
    }

    /// <summary>按命题要求生成一份试卷</summary>
    public async Task<GeneratedPaper> GenerateAsync(GeneratePaperInputDto input) =>
        input.Kind == PaperKindEnum.UnitTest
            ? await GenerateUnitPaperAsync(input)
            : await GenerateWholePaperAsync(input);

    /// <summary>普通练习卷：一次请求出完整一张卷子</summary>
    private async Task<GeneratedPaper> GenerateWholePaperAsync(GeneratePaperInputDto input)
    {
        var prompt = BuildNormalPrompt(input);
        var (content, model) = await CompleteAsync(prompt, 24000, "整卷");

        var (questions, failure) = ParseQuestions(content);
        if (questions.Count == 0)
        {
            throw new UserFriendlyException($"{ParseFailureReason(failure, content)}，请重试或改用内置样卷");
        }

        return new GeneratedPaper(ExtractTitle(content, input), input.DurationMinutes, questions, prompt, model);
    }

    /// <summary>
    /// 单元测试卷按蓝图逐大题命题再拼成整卷：整卷一次要输出十几道题还带阅读短文，
    /// 慢模型没跑完就被 HttpClient 掐断；拆成一个大题一次请求后，单次只要几分钟就能出一个大题。
    /// token 预算仍给满 24000，因为思考型模型的思维链按段计预算，给小了整段只回一个 "{"。
    /// </summary>
    private async Task<GeneratedPaper> GenerateUnitPaperAsync(GeneratePaperInputDto input)
    {
        var sections = UnitPaperBlueprint.Sections(input.Subject);
        var prompts = new StringBuilder();
        var questions = new List<GeneratedQuestion>();
        string? model = null;

        foreach (var section in sections)
        {
            var prompt = BuildSectionPrompt(input, section, sections);
            prompts.AppendLine(prompt);

            var (content, usedModel) = await CompleteAsync(prompt, 24000, section.Name);
            model ??= usedModel;

            var (parsed, failure) = ParseQuestions(content);
            if (parsed.Count == 0)
            {
                // 缺一道大题的卷子不能发给学生，宁可整卷失败让「重试」重来
                throw new UserFriendlyException($"「{section.Name}」命题失败：{ParseFailureReason(failure, content)}，请点击重试");
            }

            questions.AddRange(parsed.Select(q => q with { Section = section.Name }));
        }

        // 标题不能用模型返回的那个：每次请求只见到一道大题，它会写成「…（一、基础知识）」
        var title = $"{UnitPaperBlueprint.BuildScope(input)}{input.Subject.GetSubjectName()}单元测试卷";
        return new GeneratedPaper(title, input.DurationMinutes, questions, prompts.ToString(), model);
    }

    private const string SystemPrompt =
        "你是资深中小学命题老师，熟悉各版本教材的知识点与考试题型。" +
        "你必须只输出一个 JSON 对象，不要输出解释文字，不要使用 markdown 代码块。";

    /// <summary>调用 AI 应用的默认模型命题，把超时与空返回都换成能看懂的提示</summary>
    private async Task<(string Content, string? Model)> CompleteAsync(string prompt, int maxTokens, string stage)
    {
        BaseOutput<AiCompletionResultDto> response;
        try
        {
            response = await _aiCompletionAppService.CompleteAsync(new AiCompletionInputDto
            {
                SystemPrompt = SystemPrompt,
                UserMessage = prompt,
                // 命题需要一定的发散度但不能跑偏，0.4 兼顾题目多样性与格式稳定
                Temperature = 0.4f,
                // 思考型模型的思维链也计入 max_tokens，给小了会出现 content 只剩 "{" 而整卷解析失败
                MaxTokens = maxTokens
            });
        }
        catch (OperationCanceledException)
        {
            // 模型侧超时会被 HttpClient 包成 TaskCanceledException，直接抛出会变成一句"服务器内部错误"
            throw new UserFriendlyException($"AI 命题超时：「{stage}」十分钟仍未返回，请改用响应更快的模型，或先用内置样卷");
        }

        var content = response.Data?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new UserFriendlyException($"AI 没有返回「{stage}」的命题内容，请重试或改用内置样卷");
        }

        return (content, response.Data?.Model);
    }

    private static string ParseFailureReason(ParseFailure failure, string content)
    {
        // 带上模型原文片段，否则用户只看到"无法解析"，排查时无从下手
        var excerpt = CollapseWhitespace(content);
        if (excerpt.Length > 160) excerpt = excerpt[..160] + "…";
        var reason = failure switch
        {
            ParseFailure.TruncatedJson => "AI 输出被截断了（思考型模型的思维链会占满 token 预算）",
            ParseFailure.NoQuestions => "AI 返回的 JSON 里没有 questions 题目数组",
            _ => "AI 返回的内容不是可解析的 JSON"
        };
        return $"{reason}，模型返回片段：{excerpt}";
    }

    /// <summary>单个大题的命题请求：只出这道大题，其余大题由别的请求负责，避免模型一次写完整卷</summary>
    private static string BuildSectionPrompt(GeneratePaperInputDto input, UnitPaperBlueprint.Section section,
        List<UnitPaperBlueprint.Section> allSections)
    {
        var scope = UnitPaperBlueprint.BuildScope(input);
        var points = string.IsNullOrWhiteSpace(input.KnowledgePoints)
            ? "该单元的全部知识点（课本该单元课文、词语、句式、古诗、数学概念或英语句型）"
            : input.KnowledgePoints.Trim();

        var sb = new StringBuilder();
        sb.AppendLine($"为 {scope} 的{input.Subject.GetSubjectName()}单元测试卷命制其中一道大题：{section.Name}。");
        sb.AppendLine($"本大题要求：{section.Requirement}。");
        sb.AppendLine($"考查范围：{points}。");
        sb.AppendLine(UnitPaperBlueprint.DifficultyNote(input.Difficulty));
        sb.AppendLine($"整卷满分 100 分，共 {allSections.Count} 道大题（{string.Join(" / ", allSections.Select(x => x.Name))}），本次只出「{section.Name}」，不要越界出其它大题。");
        sb.AppendLine(UnitPaperBlueprint.SubjectNote(input.Subject));
        sb.AppendLine("其它要求：");
        sb.AppendLine("- 本大题各题分值之和必须正好等于上面给的分数，不要多也不要少；");
        sb.AppendLine($"- 每道题的 section 都固定写「{section.Name}」；");
        sb.AppendLine("- 题干要写全，需要短文的把短文直接写进题干，不要引用无法独立作答的表述；");
        sb.AppendLine();
        sb.AppendLine("严格按下面的 JSON 结构输出：");
        sb.AppendLine(FormatContract);
        AppendFieldRules(sb);
        return sb.ToString();
    }

    /// <summary>
    /// 两个命题入口共用的字段约定。编号表必须写上：
    /// 没有它时模型会照抄示例里的 "type": 1，把默写、填词甚至 25 分习作都标成选择或填空，判分方式跟着错。
    /// </summary>
    private static void AppendFieldRules(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("字段要求：");
        sb.AppendLine("- type：1 单选、2 多选、3 判断、4 填空、5 简答、6 计算、7 作文；");
        sb.AppendLine("- 只有 type 为 1 或 2 时提供 options，且必须正好 4 个，key 依次为 A/B/C/D；其它题型 options 输出空数组；");
        sb.AppendLine("- answer：单选只写一个字母；多选只写字母如 \"AC\"；判断写 \"对\" 或 \"错\"；填空题若只有一个空，可接受的答案用 \"|\" 分隔，若有多个空则按顺序用 \"；\" 分隔每个空；简答与计算给出参考答案要点；作文给出写作要求；");
        sb.AppendLine("- analysis：每题都要写，说明为什么是这个答案；");
        sb.AppendLine("- 题目按大题归类，同一 section 的题目连续排列，大题名称用 \"一、二、三、\" 中文序号。");
    }

    private const string FormatContract = """
        {
          "title": "试卷标题",
          "questions": [
            {
              "section": "一、基础知识",
              "type": 1,
              "content": "题干（选择题题干中不要写出选项）",
              "options": [ { "key": "A", "text": "选项内容" }, { "key": "B", "text": "选项内容" } ],
              "answer": "A",
              "analysis": "答案解释与知识点",
              "score": 3
            }
          ]
        }
        """;

    private static string BuildNormalPrompt(GeneratePaperInputDto input)
    {
        var subject = input.Subject.GetSubjectName();
        var difficulty = input.Difficulty switch
        {
            DifficultyEnum.Easy => "基础（覆盖课本例题与基本概念，几乎不设置陷阱）",
            DifficultyEnum.Hard => "挑战（以综合应用与易错点为主，接近竞赛或期末压轴水平）",
            _ => "提高（基础题约占六成，中等题约占三成，较难题约占一成）"
        };

        var sb = new StringBuilder();
        sb.AppendLine($"请为 {input.Grade} {subject}{input.Term}命制一份试卷。");
        sb.AppendLine($"难度：{difficulty}。");
        sb.AppendLine($"题目数量：{input.QuestionCount} 道（可以有少量出入）。");
        sb.AppendLine($"建议答题时长：{input.DurationMinutes} 分钟。");
        sb.AppendLine(string.IsNullOrWhiteSpace(input.KnowledgePoints)
            ? $"知识点范围：自行覆盖 {input.Grade} {subject} 最常见的考点。"
            : $"知识点范围：{input.KnowledgePoints}。");
        sb.AppendLine($"命题侧重：{SubjectGuidance(input.Subject)}");
        sb.AppendLine("全卷满分 100 分，各题分值之和必须恰好等于 100。");
        sb.AppendLine();
        sb.AppendLine("严格按下面的 JSON 结构输出：");
        sb.AppendLine(FormatContract);
        AppendFieldRules(sb);

        return sb.ToString();
    }

    private static string SubjectGuidance(EducationSubjectEnum subject) => subject switch
    {
        EducationSubjectEnum.Chinese =>
            "题型以字词基础、病句与文学常识、古诗文默写与理解、现代文阅读、习作组成；填空题考查按课文原文默写，不要考冷僻字。",
        EducationSubjectEnum.Math =>
            "题型以概念单选、算理填空、命题判断、计算题、解答应用题组成；计算结果必须真实正确，涉及实际情境时数据要合理。",
        EducationSubjectEnum.English =>
            "题型以词汇与语法单选、填空（含动词适当形式）、阅读理解判断、翻译或完成句子、书面表达组成；词汇与句式要符合该年级课本范围，题干使用中文说明、英文内容。",
        _ => "题型符合该学科常规考试结构。"
    };

    private enum ParseFailure
    {
        None,
        NoJson,
        TruncatedJson,
        NoQuestions
    }

    private static (List<GeneratedQuestion> Questions, ParseFailure Failure) ParseQuestions(string content)
    {
        var json = ExtractJson(content);
        if (json is null)
        {
            // 有起始括号却没配上闭合括号，说明输出被截断而不是形状不对
            var failure = content.Contains('{') || content.Contains('[') ? ParseFailure.TruncatedJson : ParseFailure.NoJson;
            return ([], failure);
        }

        JsonElement root;
        try
        {
            using var document = JsonDocument.Parse(json);
            root = document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return ([], ParseFailure.TruncatedJson);
        }

        // 有的模型直接返回题目数组，不包 questions 外壳
        JsonElement questionsNode = default;
        if (root.ValueKind == JsonValueKind.Array)
        {
            questionsNode = root;
        }
        else
        {
            var node = ReadNode(root, "questions", "items", "topics");
            if (node is not null && node.Value.ValueKind == JsonValueKind.Array) questionsNode = node.Value;
        }

        if (questionsNode.ValueKind != JsonValueKind.Array) return ([], ParseFailure.NoQuestions);

        var result = new List<GeneratedQuestion>();
        foreach (var item in questionsNode.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) continue;

            var text = ReadString(item, "content", "stem", "question", "text", "title");
            if (string.IsNullOrWhiteSpace(text)) continue;

            var options = ReadOptions(item);
            var type = ResolveType(ReadNode(item, "type"), options);
            var answer = ReadAnswer(item, type, options);

            // 模型常照抄格式示例里的 type=1，把默写、问答也标成选择题；没有选项就按题干里有没有空格降级，别把题整题丢掉
            if (type is QuestionTypeEnum.SingleChoice or QuestionTypeEnum.MultipleChoice && options.Count < 2)
            {
                type = Regex.IsMatch(text, @"_{2,}|（\s*）{1,}|\(\s*\){1,}")
                    ? QuestionTypeEnum.FillBlank
                    : QuestionTypeEnum.ShortAnswer;
            }

            // 选择题既没有选项也没有可判分的答案时只能丢弃，不能瞎猜一个
            if (string.IsNullOrWhiteSpace(answer) && type != QuestionTypeEnum.Composition) continue;

            // 判断题答案只能是对/错一类，模型有时把填词题也标成 type=3，那样自动判分必然全错，按填空处理
            if (type == QuestionTypeEnum.TrueFalse && !Regex.IsMatch(answer, @"^[对错真伪√×✓✗]|正确|错误"))
            {
                type = QuestionTypeEnum.FillBlank;
            }

            result.Add(new GeneratedQuestion(
                ReadString(item, "section", "part", "name"),
                type,
                text.Trim(),
                options,
                answer.Trim(),
                ReadString(item, "analysis", "explanation", "reason"),
                ReadScore(item, type)));
        }

        var parsed = RenumberSections(result);
        return (parsed, parsed.Count == 0 ? ParseFailure.NoQuestions : ParseFailure.None);
    }

    private static string CollapseWhitespace(string value) =>
        new StringBuilder(value.Trim())
            .Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').ToString();

    private static JsonElement? ReadNode(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)) return property.Value;
            }
        }

        return null;
    }

    private static string? ReadString(JsonElement element, params string[] names)
    {
        var node = ReadNode(element, names);
        if (node is null) return null;

        return node.Value.ValueKind switch
        {
            JsonValueKind.String => node.Value.GetString(),
            JsonValueKind.Number => node.Value.GetRawText(),
            JsonValueKind.Array => string.Join("；", node.Value.EnumerateArray()
                .Select(x => x.ValueKind == JsonValueKind.String ? x.GetString() : x.GetRawText())
                .Where(x => !string.IsNullOrWhiteSpace(x))),
            _ => null
        };
    }

    private static List<QuestionOptionDto> ReadOptions(JsonElement item)
    {
        var node = ReadNode(item, "options", "choices", "optionList");
        if (node is null || node.Value.ValueKind != JsonValueKind.Array) return [];

        var options = new List<QuestionOptionDto>();
        foreach (var option in node.Value.EnumerateArray())
        {
            string? key = null;
            string? text = null;

            if (option.ValueKind == JsonValueKind.String)
            {
                text = option.GetString();
            }
            else if (option.ValueKind == JsonValueKind.Object)
            {
                key = ReadString(option, "key", "optionKey", "label", "name");
                text = ReadString(option, "text", "content", "value", "option", "title");
                // {"label":"A","text":"..."} 里 label 是序号；只有 label 时它本身就是选项内容
                if (string.IsNullOrWhiteSpace(text)) text = key;
            }

            if (string.IsNullOrWhiteSpace(text)) continue;

            text = text.Trim();
            if (string.IsNullOrWhiteSpace(key) || key!.Length > 2)
            {
                key = LeadingOptionKey(text) ?? ((char)('A' + options.Count)).ToString();
            }

            text = StripOptionPrefix(text, key);
            if (text.Length == 0) continue;

            options.Add(new QuestionOptionDto { Key = key.ToUpperInvariant(), Text = text });
        }

        return options;
    }

    /// <summary>
    /// type 既可能是数字也可能是中文/英文描述，缺省时按有没有选项推断
    /// </summary>
    private static QuestionTypeEnum ResolveType(JsonElement? node, List<QuestionOptionDto> options)
    {
        if (node is not null)
        {
            var value = node.Value;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) &&
                Enum.IsDefined((QuestionTypeEnum)number))
            {
                return (QuestionTypeEnum)number;
            }

            if (value.ValueKind == JsonValueKind.String)
            {
                var text = value.GetString() ?? string.Empty;
                if (int.TryParse(text.Trim(), out var parsed) && Enum.IsDefined((QuestionTypeEnum)parsed))
                {
                    return (QuestionTypeEnum)parsed;
                }

                return MatchTypeByName(text);
            }
        }

        return options.Count >= 2 ? QuestionTypeEnum.SingleChoice : QuestionTypeEnum.FillBlank;
    }

    private static QuestionTypeEnum MatchTypeByName(string text)
    {
        var normalized = text.Replace(" ", "").ToLowerInvariant();
        if (normalized.Length == 0) return QuestionTypeEnum.FillBlank;

        if (normalized.Contains("多选") || normalized.Contains("multiple")) return QuestionTypeEnum.MultipleChoice;
        if (normalized.Contains("单选") || normalized.Contains("single") || normalized.Contains("choice")) return QuestionTypeEnum.SingleChoice;
        if (normalized.Contains("判断") || normalized.Contains("judge") || normalized.Contains("true") || normalized.Contains("correct")) return QuestionTypeEnum.TrueFalse;
        if (normalized.Contains("计算") || normalized.Contains("calc") || normalized.Contains("solve")) return QuestionTypeEnum.Calculation;
        if (normalized.Contains("作文") || normalized.Contains("写作") || normalized.Contains("书面表达") ||
            normalized.Contains("composition") || normalized.Contains("essay") || normalized.Contains("writing")) return QuestionTypeEnum.Composition;
        if (normalized.Contains("简答") || normalized.Contains("问答") || normalized.Contains("阅读") ||
            normalized.Contains("short") || normalized.Contains("answer") || normalized.Contains("question")) return QuestionTypeEnum.ShortAnswer;
        if (normalized.Contains("填空") || normalized.Contains("完成句子") || normalized.Contains("默写") ||
            normalized.Contains("blank") || normalized.Contains("fill")) return QuestionTypeEnum.FillBlank;

        return QuestionTypeEnum.FillBlank;
    }

    private static decimal ReadScore(JsonElement item, QuestionTypeEnum type)
    {
        var node = ReadNode(item, "score", "value", "point", "points");
        if (node is not null)
        {
            var value = node.Value;
            decimal? parsed = value.ValueKind switch
            {
                JsonValueKind.Number => value.TryGetDecimal(out var d) ? d : null,
                JsonValueKind.String => decimal.TryParse(Regex.Match(value.GetString() ?? "", @"[\d.]+").Value, out var s) ? s : null,
                _ => null
            };
            if (parsed > 0) return Math.Round(parsed.Value, 1);
        }

        return DefaultScore(type);
    }

    /// <summary>题目本身没有可靠分值时，按题型给一个常用分值</summary>
    private static decimal DefaultScore(QuestionTypeEnum type) => type switch
    {
        QuestionTypeEnum.SingleChoice or QuestionTypeEnum.TrueFalse => 3,
        QuestionTypeEnum.MultipleChoice => 4,
        QuestionTypeEnum.FillBlank => 3,
        QuestionTypeEnum.Calculation => 5,
        QuestionTypeEnum.ShortAnswer => 6,
        QuestionTypeEnum.Composition => 30,
        _ => 3
    };

    /// <summary>
    /// 答案：字母、字母数组都收；单选多选若给的是选项原文，则换成对应字母，否则没法判分
    /// </summary>
    private static string ReadAnswer(JsonElement item, QuestionTypeEnum type, List<QuestionOptionDto> options)
    {
        var answer = ReadString(item, "answer", "correctAnswer", "correct", "answerText", "result")?.Trim() ?? string.Empty;

        if (type is not (QuestionTypeEnum.SingleChoice or QuestionTypeEnum.MultipleChoice) || options.Count == 0) return answer;

        var letters = Regex.Matches(answer, @"[A-Da-d]").Select(m => m.Value.ToUpperInvariant()).Distinct().ToList();
        if (letters.Count > 0 && letters.Count <= options.Count) return string.Concat(letters.Order());

        var normalizedAnswer = NormalizeText(answer);
        var matched = options
            .Where(o => NormalizeText(o.Text).Equals(normalizedAnswer, StringComparison.OrdinalIgnoreCase))
            .Select(o => o.Key)
            .ToList();

        return matched.Count > 0 ? string.Concat(matched.Order()) : answer;
    }

    private static string NormalizeText(string value) =>
        new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray()).Trim();

    private static string? LeadingOptionKey(string text)
    {
        var match = Regex.Match(text.Trim(), @"^[（(\[]?([A-Da-d])[)）\].、．:：]");
        return match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
    }

    // 模型常把选项写成 "A. 正确" 这类带前缀的形式，展示时需要剥掉前缀
    private static string StripOptionPrefix(string text, string key) =>
        Regex.Replace(text, $@"^(\(?{Regex.Escape(key)}\)[.、．:：\s]*)", string.Empty).Trim();

    /// <summary>按出现顺序补齐大题名称，并保证同名大题的题目连续排列</summary>
    private static List<GeneratedQuestion> RenumberSections(List<GeneratedQuestion> questions)
    {
        if (questions.Count == 0) return questions;

        var sectionOrder = new List<string>();
        foreach (var question in questions)
        {
            var section = question.Section;
            if (string.IsNullOrEmpty(section)) continue;
            if (!sectionOrder.Contains(section, StringComparer.Ordinal)) sectionOrder.Add(section);
        }

        var groups = new Dictionary<string, List<GeneratedQuestion>>(StringComparer.Ordinal);
        var unassigned = new List<GeneratedQuestion>();
        foreach (var question in questions)
        {
            if (string.IsNullOrEmpty(question.Section))
            {
                unassigned.Add(question);
                continue;
            }
            if (!groups.TryGetValue(question.Section, out var list))
            {
                list = [];
                groups[question.Section] = list;
            }
            list.Add(question);
        }

        var ordered = new List<GeneratedQuestion>();
        var index = 1;
        foreach (var section in sectionOrder)
        {
            var name = $"{ChineseOrdinal(index)}、{StripSectionOrdinal(section)}";
            foreach (var question in groups[section])
            {
                ordered.Add(question with { Section = name });
            }
            index++;
        }

        var tailName = ordered.Count == 0 ? "一、试题" : $"{ChineseOrdinal(index)}、综合练习";
        foreach (var question in unassigned)
        {
            ordered.Add(question with { Section = tailName });
        }

        return ordered;
    }

    private static string StripSectionOrdinal(string section) =>
        Regex.Replace(section.Trim(), @"^[一二三四五六七八九十]*[、.．]\s*", string.Empty);

    private static string ChineseOrdinal(int number) => number switch
    {
        <= 0 or > 20 => number.ToString(),
        < 11 => "一二三四五六七八九十"[number - 1].ToString(),
        < 20 => "十" + "一二三四五六七八九"[number - 11],
        _ => "二十"
    };

    private static string ExtractTitle(string content, GeneratePaperInputDto input)
    {
        var json = ExtractJson(content);
        if (json is not null)
        {
            try
            {
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    var title = ReadString(root, "title", "paperTitle", "name");
                    if (!string.IsNullOrWhiteSpace(title)) return title.Trim();
                }
            }
            catch (JsonException)
            {
                // 标题不是必要条件，解析失败时退回默认标题
            }
        }

        var term = string.IsNullOrWhiteSpace(input.Term) ? string.Empty : input.Term;
        return input.Kind == PaperKindEnum.UnitTest
            ? $"{UnitPaperBlueprint.BuildScope(input)}{input.Subject.GetSubjectName()}单元测试卷"
            : $"{input.Grade}{input.Subject.GetSubjectName()}{term}练习卷";
    }

    /// <summary>从模型输出里截出第一个完整的 JSON 对象或数组，容忍包裹的说明文字与代码块</summary>
    private static string? ExtractJson(string content)
    {
        var start = content.IndexOfAny(['{', '[']);
        if (start < 0) return null;

        var open = content[start];
        var close = open == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var escaped = false;

        for (var i = start; i < content.Length; i++)
        {
            var c = content[i];

            if (inString)
            {
                if (escaped) escaped = false;
                else if (c == '\\') escaped = true;
                else if (c == '"') inString = false;
                continue;
            }

            switch (c)
            {
                case '"': inString = true; break;
                case '{': case '[': depth++; break;
                case '}': case ']':
                    depth--;
                    if (depth == 0 && c == close) return content[start..(i + 1)];
                    break;
            }
        }

        return null;
    }
}
