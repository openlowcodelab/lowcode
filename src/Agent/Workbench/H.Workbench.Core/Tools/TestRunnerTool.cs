using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 测试自检工具：把"我改完了"变成"测试 12 通过 / 1 失败"。
/// 与 workspace_shell 的区别是命令由服务端拼装、模型只能给仓库与筛选条件，
/// 因此不必像自由命令那样每次都要人工审批——否则员工不会去跑测试。
/// 结果从 .trx 解析：控制台文本在中文环境输出"失败!"，按文本计数迟早骗人。
/// </summary>
public class TestRunnerTool
{
    private static readonly XNamespace TrxNs = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

    private readonly WorkbenchToolOptions _options;
    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<TestRunnerTool> _logger;

    public TestRunnerTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<TestRunnerTool> logger)
    {
        _options = options.Value;
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("在已克隆仓库内执行 dotnet test 自检（命令由服务端拼装，不接受自由命令）。参数：repo, project（测试项目或子目录相对路径，留空=仓库根）, filter（dotnet --filter 表达式，可空）, timeoutSeconds。")]
    public async Task<string> WorkspaceRunTestsAsync(
        [Description("已克隆的仓库目录名或仓库地址")] string repo,
        [Description("仓库内测试项目/子目录相对路径，留空则在仓库根执行")] string? project = null,
        [Description("用例筛选表达式，如 FullyQualifiedName~Login；留空跑全部")] string? filter = null,
        [Description("超时秒数，默认 600 秒")] int timeoutSeconds = 600,
        CancellationToken cancellationToken = default)
    {
        if (!_resolver.TryResolveRepo(repo, requireExisting: true, out var repoDir, out var resolveError))
        {
            return Fail(resolveError ?? $"仓库不存在或越出工作目录: {repo}");
        }

        var targetDir = repoDir!;
        if (!string.IsNullOrWhiteSpace(project))
        {
            if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, project, out var inside, out var pathError))
            {
                return Fail(pathError!);
            }

            targetDir = Directory.Exists(inside!) ? inside! : Path.GetDirectoryName(inside!)!;
        }

        // filter 会进 shell 命令串：只留安全字符，杜绝借筛选条件注入命令
        string? safeFilter = null;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            safeFilter = new string(filter.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '~' or '=' or '|' or '(' or ')' or ' ' or '^').ToArray()).Trim();
            if (safeFilter.Length == 0) return Fail("filter 含非法字符");
        }

        var trxName = $"agent-{Guid.NewGuid():N}.trx";
        var command = $"dotnet test --nologo --logger \"trx;LogFileName={trxName}\""
                      + (safeFilter is null ? "" : $" --filter \"{safeFilter}\"");

        var timeout = Math.Clamp(
            timeoutSeconds > 0 ? timeoutSeconds : 600,
            1, Math.Max(30, _options.ToolTimeoutSeconds - 10));

        _logger.LogInformation("工作区执行测试: {Repo} ({Dir}), timeout={Timeout}s",
            Path.GetFileName(repoDir!), targetDir, timeout);

        ShellRunResult result;
        using (await _locks.AcquireAsync(repoDir!, cancellationToken))
        {
            result = await ShellRunner.RunAsync(targetDir, command, usePwsh: false, timeout,
                _options.Shell.MaxOutputChars, cancellationToken);
        }

        if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.Error))
        {
            return Fail(result.Error);
        }

        var trxPath = FindTrx(repoDir!, trxName);
        var (passed, failed, skipped, total, failures) = trxPath is null
            ? (0, 0, 0, 0, Array.Empty<string>())
            : ParseTrx(trxPath);

        if (total == 0)
        {
            return Fail($"未能解析出测试结果（.trx {(trxPath is null ? "未生成" : "无用例")}）。" +
                        "该仓库可能没有测试项目；dotnet 输出尾部：" + Tail(result.StdOut + result.StdErr));
        }

        var success = result.ExitCode == 0 && failed == 0;

        return Ok(new
        {
            repo = Path.GetFileName(repoDir!),
            exitCode = result.ExitCode,
            durationMs = result.DurationMs,
            passed,
            failed,
            skipped,
            total,
            failures = failures.Length > 0 ? failures : null,
            stdoutTail = Tail(result.StdOut),
            stderrTail = Tail(result.StdErr)
        });
    }

    private static string? FindTrx(string repoDir, string trxName)
    {
        try
        {
            return Directory.EnumerateFiles(repoDir, trxName, SearchOption.AllDirectories).FirstOrDefault();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static (int Passed, int Failed, int Skipped, int Total, string[] Failures) ParseTrx(string path)
    {
        try
        {
            var doc = XDocument.Load(path);
            var results = doc.Root?.Descendants(TrxNs + "UnitTestResult").ToList() ?? [];

            var passed = results.Count(r => (string?)r.Attribute("outcome") == "Passed");
            var failedList = results.Where(r => (string?)r.Attribute("outcome") == "Failed").ToList();
            var skipped = results.Count(r => (string?)r.Attribute("outcome") == "NotExecuted");

            var names = failedList.Select(r => (string?)r.Attribute("testName") ?? "")
                .Where(n => n.Length > 0)
                .Take(8)
                .ToArray();

            return (passed, failedList.Count, skipped, results.Count, names);
        }
        catch (Exception ex)
        {
            // 解析失败要能被看见，但不能让整个工具崩掉
            return (0, 0, 0, 0, [$"trx 解析失败: {ex.Message}"]);
        }
    }

    private static string Tail(string? text, int max = 1200)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var trimmed = text.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[^max..];
    }

    private static string Ok(object payload) =>
        JsonSerializer.Serialize(new { success = true, data = payload }, JsonOptions);

    private static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error = GitRunner.MaskCredentials(error) }, JsonOptions);

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };
}
