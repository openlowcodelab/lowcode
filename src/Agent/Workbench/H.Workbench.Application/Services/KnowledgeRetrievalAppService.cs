using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using System.Linq.Expressions;

namespace H.Workbench.Application;

/// <summary>
/// 知识库轻量检索：分词 → SQL LIKE 召回 → 内存打分 → TopN 片段。
/// 无向量库/embedding，纯关键词，供 AgentFactory 运行时注入提示词。
/// </summary>
public class KnowledgeRetrievalAppService : ApplicationService, IKnowledgeRetrievalAppService
{
    private const int MaxTerms = 12;
    private const int RecallLimit = 60;

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "的", "了", "和", "是", "在", "我", "有", "你", "他", "她", "它", "们", "这", "那", "怎么", "如何", "什么",
        "the", "a", "an", "and", "or", "is", "are", "to", "of", "in", "on", "for", "with", "how", "what"
    };

    private readonly IRepository<KnowledgeNodeEntity, Guid> _nodeRepository;
    private readonly IRepository<KnowledgeDocumentEntity, Guid> _documentRepository;
    private readonly IRepository<KnowledgeBaseEntity, Guid> _baseRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public KnowledgeRetrievalAppService(
        IRepository<KnowledgeNodeEntity, Guid> nodeRepository,
        IRepository<KnowledgeDocumentEntity, Guid> documentRepository,
        IRepository<KnowledgeBaseEntity, Guid> baseRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _nodeRepository = nodeRepository;
        _documentRepository = documentRepository;
        _baseRepository = baseRepository;
        _asyncExecuter = asyncExecuter;
    }

    public async Task<BaseOutput<List<KnowledgeSnippetDto>>> SearchAsync(SearchKnowledgeInput input)
    {
        var empty = new List<KnowledgeSnippetDto>();
        if (input.KnowledgeBaseIds == null || input.KnowledgeBaseIds.Count == 0 ||
            string.IsNullOrWhiteSpace(input.Query))
        {
            return new(empty);
        }

        var terms = Tokenize(input.Query);
        if (terms.Count == 0) return new(empty);

        var kbIds = input.KnowledgeBaseIds.Distinct().ToHashSet();

        // 命中知识库下的文档节点
        var nodeQuery = await _nodeRepository.GetQueryableAsync();
        var nodes = await _asyncExecuter.ToListAsync(
            nodeQuery.Where(n => n.OwnerType == OwnerTypes.Knowledge
                                 && n.NodeType == "Document"
                                 && n.KnowledgeBaseId != null
                                 && kbIds.Contains(n.KnowledgeBaseId.Value))
                .Select(n => new { n.Id, n.Title, KbId = n.KnowledgeBaseId!.Value }));

        if (nodes.Count == 0) return new(empty);

        var nodeIds = nodes.Select(n => n.Id).ToList();
        var nodeById = nodes.ToDictionary(n => n.Id);

        // SQL LIKE 召回：文档内容包含任一关键词
        var docQuery = await _documentRepository.GetQueryableAsync();
        Expression<Func<KnowledgeDocumentEntity, bool>> like = BuildLikePredicate(terms);
        var candidates = await _asyncExecuter.ToListAsync(
            docQuery.Where(d => d.NodeId != null && nodeIds.Contains(d.NodeId.Value)
                                && d.Content != null && d.Content != "")
                .Where(like)
                .Select(d => new { d.NodeId, d.Content })
                .Take(RecallLimit));

        if (candidates.Count == 0) return new(empty);

        var baseNames = await GetBaseNamesAsync(kbIds);

        var scored = new List<(KnowledgeSnippetDto dto, double score)>();
        foreach (var c in candidates)
        {
            if (c.NodeId == null || !nodeById.TryGetValue(c.NodeId.Value, out var node)) continue;
            var content = c.Content ?? string.Empty;

            double score = 0;
            foreach (var term in terms)
            {
                var titleHits = CountOccurrences(node.Title, term);
                var contentHits = CountOccurrences(content, term);
                score += titleHits * 10 + Math.Min(contentHits, 20);
            }
            score /= 1 + content.Length / 20000.0; // 长度惩罚

            if (score <= 0) continue;

            scored.Add((new KnowledgeSnippetDto
            {
                NodeId = node.Id,
                KnowledgeBaseId = node.KbId,
                KnowledgeBaseName = baseNames.TryGetValue(node.KbId, out var nm) ? nm : string.Empty,
                Title = node.Title,
                Snippet = ExtractSnippet(content, terms, input.SnippetMaxChars),
                Score = score
            }, score));
        }

        var top = scored
            .OrderByDescending(x => x.score)
            .Take(Math.Clamp(input.TopN, 1, 10))
            .Select(x => x.dto)
            .ToList();

        return new(top);
    }

    private async Task<Dictionary<Guid, string>> GetBaseNamesAsync(HashSet<Guid> kbIds)
    {
        var query = await _baseRepository.GetQueryableAsync();
        var list = await _asyncExecuter.ToListAsync(
            query.Where(b => kbIds.Contains(b.Id)).Select(b => new { b.Id, b.Name }));
        return list.ToDictionary(x => x.Id, x => x.Name);
    }

    private static Expression<Func<KnowledgeDocumentEntity, bool>> BuildLikePredicate(List<string> terms)
    {
        var param = Expression.Parameter(typeof(KnowledgeDocumentEntity), "d");
        var contentProp = Expression.Property(param, nameof(KnowledgeDocumentEntity.Content));
        var efFunctions = Expression.Property(null,
            typeof(EF).GetProperty(nameof(EF.Functions))!);
        var likeMethod = typeof(DbFunctionsExtensions)
            .GetMethods()
            .First(m => m.Name == nameof(DbFunctionsExtensions.Like)
                        && m.GetParameters().Length == 3
                        && m.GetParameters()[1].ParameterType == typeof(string)
                        && m.GetParameters()[2].ParameterType == typeof(string));

        Expression? body = null;
        foreach (var term in terms)
        {
            var pattern = Expression.Constant($"%{EscapeLike(term)}%");
            var call = Expression.Call(likeMethod, efFunctions, contentProp, pattern);
            body = body == null ? call : Expression.OrElse(body, call);
        }

        return Expression.Lambda<Func<KnowledgeDocumentEntity, bool>>(body!, param);
    }

    private static string EscapeLike(string term) =>
        term.Replace("%", "[%]").Replace("_", "[_]");

    internal static List<string> Tokenize(string query)
    {
        // 用户消息常以指令前缀开头（"不要使用任何工具。直接回答：…"），
        // 信息焦点在尾部——超长时只取尾部 100 字符再分词，避免关键词被词数上限挤出
        if (query.Length > 100) query = query[^100..];

        var terms = new List<string>();
        var tokenChars = new List<char>();
        var cjkRun = new List<char>();

        void FlushCjk()
        {
            if (cjkRun.Count == 0) return;
            // 中文按 2-gram 滑窗
            if (cjkRun.Count == 1) terms.Add(new string(cjkRun.ToArray()));
            for (var i = 0; i + 1 <= cjkRun.Count; i++)
                terms.Add(new string(cjkRun.Skip(i).Take(2).ToArray()));
            cjkRun.Clear();
        }

        void FlushToken()
        {
            if (tokenChars.Count == 0) return;
            var tok = new string(tokenChars.ToArray());
            if (!StopWords.Contains(tok)) terms.Add(tok.ToLowerInvariant());
            tokenChars.Clear();
        }

        foreach (var ch in query)
        {
            var isCjk = ch >= 0x4E00 && ch <= 0x9FFF;
            if (isCjk)
            {
                FlushToken();
                cjkRun.Add(ch);
            }
            else if (char.IsLetterOrDigit(ch))
            {
                FlushCjk();
                tokenChars.Add(ch);
            }
            else
            {
                FlushToken();
                FlushCjk();
            }
        }

        FlushToken();
        FlushCjk();

        var distinct = terms.Where(t => t.Length > 0).Distinct().ToList();
        // 取尾部词而非头部：指令前缀（"请不要使用工具直接回答"）占头部，主题词在尾部
        return distinct.Count <= MaxTerms ? distinct : distinct[^MaxTerms..];
    }

    private static int CountOccurrences(string text, string term)
    {
        if (string.IsNullOrEmpty(term)) return 0;
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(term, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            idx += term.Length;
        }
        return count;
    }

    private static string ExtractSnippet(string content, List<string> terms, int maxChars)
    {
        var cap = Math.Clamp(maxChars, 200, 2000);
        var lower = content.ToLowerInvariant();

        var hitPos = -1;
        foreach (var term in terms)
        {
            var p = lower.IndexOf(term.ToLowerInvariant(), StringComparison.Ordinal);
            if (p >= 0 && (hitPos < 0 || p < hitPos)) hitPos = p;
        }

        if (hitPos < 0)
        {
            return content.Length <= cap ? content : content[..cap] + "…";
        }

        var window = cap / 2;
        var start = Math.Max(0, hitPos - window);
        var end = Math.Min(content.Length, start + cap);
        var snippet = content[start..end];

        // Markdown 标题行优先：若截断处非行首，回退到最近换行
        if (start > 0)
        {
            var nl = snippet.IndexOf('\n');
            if (nl >= 0 && nl < 40) snippet = snippet[(nl + 1)..];
        }

        return snippet;
    }
}
