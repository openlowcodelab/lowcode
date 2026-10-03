using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 构建与脚手架工具：回答"这份代码到底编得过编不过"。
/// 与 workspace_shell 的分工是命令由服务端拼装、模型只给仓库/子路径/配置名，
/// 所以不必像自由命令那样每次人工审批——否则员工根本不会去编译，只会在"应该能跑"上签字。
/// </summary>
public class WorkspaceBuildTool
{
    /// <summary>
    /// dotnet new 模板短名白名单。不接受任意字符串，防止借模板名往命令里塞参数。
    /// </summary>
    private static readonly string[] AllowedTemplates =
    [
        "console", "classlib", "xunit", "mstest", "nunit", "web", "mvc", "razor",
        "blazor", "blazorwasm", "wpf", "winforms", "maui", "sln", "nugetconfig", "gitignore"
    ];

    private static readonly string[] AllowedConfigurations = ["Debug", "Release"];

    /// <summary>
    /// MSBuild/dotnet 的错误行形如 `Program.cs(12,5): error CS1002: ; expected`。
    /// 按错误码正则抓而不是按"失败/成功"文本数：中文环境的控制台文案会变，错误码不会。
    /// </summary>
    private static readonly Regex ErrorLineRegex = new(
        @"\berror\s+[A-Z]{2,}\d{3,}\b[^\r\n]*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WarningRegex = new(
        @"\bwarning\s+[A-Z]{2,}\d{3,}\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly WorkbenchToolOptions _options;
    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<WorkspaceBuildTool> _logger;

    public WorkspaceBuildTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<WorkspaceBuildTool> logger)
    {
        _options = options.Value;
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("编译已克隆仓库（dotnet build，命令由服务端拼装）。返回退出码与错误清单，构建未通过时 success=false。参数：repo, project（仓库内 .csproj/.sln 或子目录相对路径，留空=仓库根）, configuration（Debug/Release）, timeoutSeconds。")]
    public async Task<string> WorkspaceBuildAsync(
        [Description("已克隆的仓库目录名或仓库地址")] string repo,
        [Description("仓库内项目/子目录相对路径，留空则在仓库根编译")] string? project = null,
        [Description("编译配置 Debug 或 Release，默认 Debug")] string? configuration = "Debug",
        [Description("超时秒数，默认 600 秒")] int timeoutSeconds = 600,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveTarget(repo, project, out var repoDir, out var targetDir, out var projectArg, out var error))
        {
            return ToolEnvelope.Fail(error!);
        }

        var config = NormalizeConfiguration(configuration);

        var command = $"dotnet build {projectArg} --nologo -c {config}";
        var result = await RunLockedAsync(repoDir!, targetDir!, command, timeoutSeconds, cancellationToken);

        if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.Error))
        {
            return ToolEnvelope.Fail(result.Error!);
        }

        var errors = ExtractErrors(result.StdOut + Environment.NewLine + result.StdErr);
        var succeeded = result.ExitCode == 0 && errors.Count == 0;

        var payload = new
        {
            repo = Path.GetFileName(repoDir!),
            target = Path.GetRelativePath(repoDir!, targetDir!).Replace('\\', '/'),
            configuration = config,
            exitCode = result.ExitCode,
            durationMs = result.DurationMs,
            errorCount = errors.Count,
            warningCount = WarningRegex.Matches(result.StdOut + result.StdErr).Count,
            errors = errors.Count > 0 ? errors.Take(15).ToArray() : null,
            stdoutTail = Tail(result.StdOut),
            stderrTail = Tail(result.StdErr)
        };

        return succeeded
            ? ToolEnvelope.Ok(payload)
            : ToolEnvelope.Fail($"构建未通过（退出码 {result.ExitCode}，{errors.Count} 个错误）：" +
                                (errors.Count > 0 ? string.Join(" | ", errors.Take(5)) : Tail(result.StdErr)));
    }

    [Description("还原仓库的 NuGet 依赖（dotnet restore）。构建报缺包时先调这个。参数：repo, project（相对路径，留空=仓库根）, timeoutSeconds。")]
    public async Task<string> WorkspaceRestoreAsync(
        [Description("已克隆的仓库目录名或仓库地址")] string repo,
        [Description("仓库内项目/子目录相对路径，留空则在仓库根还原")] string? project = null,
        [Description("超时秒数，默认 600 秒")] int timeoutSeconds = 600,
        CancellationToken cancellationToken = default)
    {
        if (!TryResolveTarget(repo, project, out var repoDir, out var targetDir, out var projectArg, out var error))
        {
            return ToolEnvelope.Fail(error!);
        }

        var result = await RunLockedAsync(repoDir!, targetDir!, $"dotnet restore {projectArg} --nologo",
            timeoutSeconds, cancellationToken);

        if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.Error))
        {
            return ToolEnvelope.Fail(result.Error!);
        }

        var errors = ExtractErrors(result.StdOut + Environment.NewLine + result.StdErr);
        var payload = new
        {
            repo = Path.GetFileName(repoDir!),
            exitCode = result.ExitCode,
            durationMs = result.DurationMs,
            errors = errors.Count > 0 ? errors.Take(15).ToArray() : null,
            stdoutTail = Tail(result.StdOut)
        };

        return result.ExitCode == 0 && errors.Count == 0
            ? ToolEnvelope.Ok(payload)
            : ToolEnvelope.Fail($"依赖还原未通过（退出码 {result.ExitCode}）：" +
                                (errors.Count > 0 ? string.Join(" | ", errors.Take(5)) : Tail(result.StdErr)));
    }

    [Description("在已克隆仓库内用 dotnet new 生成工程骨架（模板由服务端白名单校验，只允许 console/classlib/xunit/nunit/mstest/web/mvc/razor/blazor/blazorwasm/wpf/winforms/maui/sln/nugetconfig/gitignore）。参数：repo, template（模板短名）, name（工程名，将作为目录与程序集名）, directory（仓库内放置位置，留空=仓库根）, timeoutSeconds。")]
    public async Task<string> WorkspaceScaffoldAsync(
        [Description("已克隆的仓库目录名或仓库地址")] string repo,
        [Description("dotnet new 模板短名，须在白名单内")] string template,
        [Description("工程名称，只允许字母数字与 . _ -")] string name,
        [Description("仓库内目标相对目录，留空则放在仓库根")] string? directory = null,
        [Description("超时秒数，默认 180 秒")] int timeoutSeconds = 180,
        CancellationToken cancellationToken = default)
    {
        if (!_resolver.TryResolveRepo(repo, requireExisting: true, out var repoDir, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError ?? $"仓库不存在或越出工作目录: {repo}");
        }

        var shortName = (template ?? "").Trim().ToLowerInvariant();
        if (Array.IndexOf(AllowedTemplates, shortName) < 0)
        {
            return ToolEnvelope.Fail($"模板 {template} 不在允许列表内，可用模板：" + string.Join(", ", AllowedTemplates));
        }

        var safeName = (name ?? "").Trim();
        if (safeName.Length == 0 || !safeName.All(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'))
        {
            return ToolEnvelope.Fail("name 只能包含字母、数字与 . _ -");
        }

        var outputDir = string.IsNullOrWhiteSpace(directory) ? "." : directory.Trim();
        if (!IsSafeRelative(outputDir))
        {
            return ToolEnvelope.Fail($"directory 含非法字符或被判定越出仓库: {directory}");
        }

        if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, outputDir, out var absOut, out var pathError))
        {
            return ToolEnvelope.Fail(pathError!);
        }

        var command = $"dotnet new {shortName} -o {outputDir} -n {safeName}";
        var result = await RunLockedAsync(repoDir!, repoDir!, command, timeoutSeconds, cancellationToken);

        if (result.ExitCode == -1 && !string.IsNullOrEmpty(result.Error))
        {
            return ToolEnvelope.Fail(result.Error!);
        }

        var created = new List<string>();
        try
        {
            created = Directory.EnumerateFileSystemEntries(absOut!)
                .Select(p => Path.GetRelativePath(repoDir!, p).Replace('\\', '/'))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .Take(40)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "脚手架产物清单读取失败: {Dir}", absOut);
        }

        return result.ExitCode == 0
            ? ToolEnvelope.Ok(new
            {
                repo = Path.GetFileName(repoDir!),
                template = shortName,
                name = safeName,
                directory = outputDir == "." ? "." : outputDir.Replace('\\', '/'),
                created,
                stdoutTail = Tail(result.StdOut)
            })
            : ToolEnvelope.Fail($"脚手架生成未通过（退出码 {result.ExitCode}）：" +
                                Tail(result.StdOut + result.StdErr));
    }

    private async Task<ShellRunResult> RunLockedAsync(
        string repoDir, string workingDir, string command, int timeoutSeconds, CancellationToken ct)
    {
        var timeout = Math.Clamp(
            timeoutSeconds > 0 ? timeoutSeconds : 600,
            1, Math.Max(30, _options.ToolTimeoutSeconds - 10));

        _logger.LogInformation("工作区构建命令: {Repo} ({Dir}) -> {Command}, timeout={Timeout}s",
            Path.GetFileName(repoDir), workingDir, command, timeout);

        using (await _locks.AcquireAsync(repoDir, ct))
        {
            return await ShellRunner.RunAsync(workingDir, command, usePwsh: false, timeout,
                _options.Shell.MaxOutputChars, ct);
        }
    }

    private bool TryResolveTarget(
        string repo, string? project,
        out string? repoDir, out string? targetDir, out string projectArg, out string? error)
    {
        repoDir = targetDir = null;
        projectArg = ".";
        error = null;

        if (!_resolver.TryResolveRepo(repo, requireExisting: true, out var resolved, out var resolveError))
        {
            error = resolveError ?? $"仓库不存在或越出工作目录: {repo}";
            return false;
        }

        repoDir = resolved;
        targetDir = resolved;

        if (string.IsNullOrWhiteSpace(project)) return true;

        var rel = project.Trim();
        if (!IsSafeRelative(rel))
        {
            error = $"project 含非法字符或被判定越出仓库: {project}";
            return false;
        }

        if (!GitWorkspaceResolver.TryResolveInRepo(resolved!, rel, out var inside, out var pathError))
        {
            error = pathError!;
            return false;
        }

        // 目录存在就在目录内执行、只给 "."；文件（csproj/sln）则把相对路径交给 dotnet
        if (Directory.Exists(inside!))
        {
            targetDir = inside;
            return true;
        }

        if (!File.Exists(inside!))
        {
            error = $"项目路径不存在: {project}（可先用 WorkspaceListFilesAsync 确认）";
            return false;
        }

        targetDir = resolved;
        projectArg = rel.Replace('\\', '/');
        return true;
    }

    /// <summary>
    /// cmd.exe 侧的参数护栏：引号、与或、重定向、插入符一律拒绝；空格也拒绝——
    /// 加了引号反而会被 cmd /s 原样传给 dotnet（实测得到 "." 这种带引号的路径），
    /// 不带引号又不含空格才是这里唯一可靠的写法。路径遍历交给 GitWorkspaceResolver 校验。
    /// </summary>
    private static bool IsSafeRelative(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Contains("..", StringComparison.Ordinal)) return false;
        if (value.Any(c => c is '"' or '\'' or '&' or '|' or '<' or '>' or '^' or '%' or '$' or ';' or '`' or ' ')) return false;
        return value.All(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-' or '/' or '\\' or '+');
    }

    private static string NormalizeConfiguration(string? configuration)
    {
        var value = (configuration ?? "").Trim();
        var match = AllowedConfigurations.FirstOrDefault(c =>
            string.Equals(c, value, StringComparison.OrdinalIgnoreCase));
        return match ?? "Debug";
    }

    private static List<string> ExtractErrors(string text)
    {
        var lines = new List<string>();
        foreach (Match m in ErrorLineRegex.Matches(text))
        {
            var line = m.Value.Trim();
            if (line.Length > 0 && !lines.Contains(line, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add(line);
            }
        }

        return lines;
    }

    private static string Tail(string? text, int max = 1200)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var trimmed = text.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[^max..];
    }
}
