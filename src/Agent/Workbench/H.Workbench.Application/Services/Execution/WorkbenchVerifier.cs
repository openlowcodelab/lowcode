using H.Workbench.Core;
using H.Workbench.Core.Agents;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Volo.Abp.DependencyInjection;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 验收裁决结果。Verdict 只取 Pass/Fail/Unclear。
/// </summary>
public sealed record VerificationOutcome(string Verdict, string? Reason, int PromptTokens = 0, int CompletionTokens = 0);

/// <summary>
/// 结果验收器：员工声称"做完了"之后，由一次不带任何工具的 LLM 调用
/// 按任务上登记的验收标准复核，把"完成"从员工的说法变成有判据的结论。
/// 没有验收标准时不裁决——无判据的裁决只是第二个幻觉。
/// </summary>
public class WorkbenchVerifier : ITransientDependency
{
    private readonly AgentFactory _agentFactory;
    private readonly WorkbenchToolOptions _options;
    private readonly ILogger<WorkbenchVerifier> _logger;

    public WorkbenchVerifier(
        AgentFactory agentFactory,
        Microsoft.Extensions.Options.IOptions<WorkbenchToolOptions> options,
        ILogger<WorkbenchVerifier> logger)
    {
        _agentFactory = agentFactory;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 返回 null 表示"未裁决"（功能关闭、无验收标准、模型不可用或裁决自身失败），
    /// 调用方不得把未裁决当成通过。
    /// </summary>
    public async Task<VerificationOutcome?> VerifyAsync(
        string? acceptanceCriteria,
        string goal,
        string result,
        IReadOnlyList<string> evidence,
        CancellationToken ct = default)
    {
        if (!_options.Verification.Enabled || string.IsNullOrWhiteSpace(acceptanceCriteria))
        {
            return null;
        }

        var provider = await _agentFactory.CreateReviewProviderAsync();
        if (provider == null)
        {
            _logger.LogWarning("无可用 LLM 配置，跳过结果验收");
            return null;
        }

        var maxChars = Math.Max(500, _options.Verification.MaxEvidenceChars);

        var sb = new StringBuilder();
        sb.AppendLine("## 验收标准");
        sb.AppendLine(Trim(acceptanceCriteria, maxChars));
        sb.AppendLine();
        sb.AppendLine("## 本次任务目标");
        sb.AppendLine(Trim(goal, 1000));
        sb.AppendLine();
        sb.AppendLine("## 员工声称的执行结果");
        sb.AppendLine(Trim(result, maxChars));
        if (evidence.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("## 过程留下的可核验证据（工具产出）");
            foreach (var line in evidence.Take(20))
            {
                sb.AppendLine($"- {line}");
            }
        }

        try
        {
            var request = new LLMRequest
            {
                Messages =
                [
                    new Message
                    {
                        Role = "system",
                        Content = "你是严格的验收裁决员，只依据给定的验收标准判断执行结果是否达标，" +
                                  "不替员工找借口、不臆测未写出的事实。" +
                                  "证据不足以支撑结论时判 Unclear。" +
                                  "只输出一行 JSON，不要 markdown 代码块：{\"verdict\":\"Pass|Fail|Unclear\",\"reason\":\"不超过80字的裁决依据\"}"
                    },
                    new Message { Role = "user", Content = sb.ToString() }
                ],
                Temperature = 0.1f,
                // 思考型模型会把预算耗在推理段上，300 常导致可见内容为空、裁决静默失败
                MaxTokens = 800
            };

            var response = await provider.ChatAsync(request, ct);
            var outcome = Parse(response.Content);
            if (outcome is null)
            {
                _logger.LogWarning("验收裁决无法解析，按未裁决处理: {Raw}", Trim(response.Content, 300));
                return null;
            }

            return outcome with
            {
                PromptTokens = response.PromptTokens,
                CompletionTokens = response.CompletionTokens
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "结果验收调用失败，按未裁决处理");
            return null;
        }
    }

    private static VerificationOutcome? Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            using var doc = JsonDocument.Parse(content[start..(end + 1)]);
            var root = doc.RootElement;
            var verdict = root.TryGetProperty("verdict", out var v) ? v.GetString() : null;
            var reason = root.TryGetProperty("reason", out var r) && r.ValueKind == JsonValueKind.String
                ? r.GetString()
                : null;

            return verdict?.Trim().ToLowerInvariant() switch
            {
                "pass" => new VerificationOutcome("Pass", Trim(reason, 500), 0),
                "fail" => new VerificationOutcome("Fail", Trim(reason, 500), 0),
                "unclear" => new VerificationOutcome("Unclear", Trim(reason, 500), 0),
                _ => null
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? Trim(string? text, int max)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= max) return text;
        return text[..max] + "…";
    }
}
