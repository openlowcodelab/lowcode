using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 工作区 shell 工具：在已克隆仓库目录内执行命令（构建/测试等）。
/// 护栏：目录经 GitWorkspaceResolver 白名单、与 git 工具同键互斥锁、超时 clamp、输出截断、
/// 全局开关 Workbench:Shell:Enabled；种子默认 RequiresApproval=true（人工批准后执行）。
/// 实例类：由 DI 单例注册（红线：只依赖单例）。
/// </summary>
public class WorkspaceShellTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly WorkbenchToolOptions _options;
    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<WorkspaceShellTool> _logger;

    public WorkspaceShellTool(
        IOptions<WorkbenchToolOptions> options,
        GitWorkspaceLocks locks,
        ILogger<WorkspaceShellTool> logger)
    {
        _options = options.Value;
        _resolver = new GitWorkspaceResolver(options);
        _locks = locks;
        _logger = logger;
    }

    [Description("在工作区内指定仓库目录下执行 shell 命令（如构建、测试），受超时与输出截断限制，需人工审批。参数：repo, command, timeoutSeconds, usePwsh。")]
    public async Task<string> WorkspaceRunShellAsync(
        [Description("已克隆的仓库目录名或仓库地址（命令的工作目录）")] string repo,
        [Description("要执行的完整命令文本")] string command,
        [Description("超时秒数，默认取服务端配置")] int timeoutSeconds = 0,
        [Description("true 使用 PowerShell(pwsh)，false 使用系统默认 shell")] bool usePwsh = false,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Shell.Enabled)
        {
            return Fail("workspace_shell 已在服务端配置中禁用（Workbench:Shell:Enabled=false）");
        }

        if (string.IsNullOrWhiteSpace(command))
        {
            return Fail("command 不能为空");
        }

        string? repoDir;
        if (!string.IsNullOrWhiteSpace(repo))
        {
            if (!_resolver.TryResolveRepo(repo, requireExisting: true, out repoDir, out var resolveError))
            {
                return Fail(resolveError ?? $"仓库不存在或越出工作目录: {repo}");
            }
        }
        else if (!_resolver.TryGetRoot(out repoDir, out var rootError))
        {
            return Fail(rootError!);
        }

        // 与 GitTool 同键（仓库根目录）互斥，防止 shell 与 git 写操作并发破坏工作副本
        using (await _locks.AcquireAsync(repoDir!, cancellationToken))
        {
            // 必须小于 ToolExecutor 兜底超时（660s），否则被兜底掐断丢失输出
            var timeout = Math.Clamp(
                timeoutSeconds > 0 ? timeoutSeconds : _options.Shell.DefaultTimeoutSeconds,
                1, Math.Min(_options.Shell.MaxTimeoutSeconds, _options.ToolTimeoutSeconds - 10));

            _logger.LogInformation("工作区执行命令: {Repo} $ {Command} (timeout={Timeout}s, pwsh={Pwsh})",
                Path.GetFileName(repoDir!), GitRunner.MaskCredentials(command), timeout, usePwsh);

            var result = await ShellRunner.RunAsync(repoDir!, command, usePwsh, timeout,
                _options.Shell.MaxOutputChars, cancellationToken);

            if (result.Error is not null && result.ExitCode == -1 && string.IsNullOrEmpty(result.StdErr))
            {
                return Fail(result.Error);
            }

            // 关键信息（exitCode/耗时）放前部，防被 ToolExecutor 二次截断挤掉
            return Ok(new
            {
                repo = Path.GetFileName(repoDir!),
                command,
                cwd = repoDir,
                exitCode = result.ExitCode,
                durationMs = result.DurationMs,
                truncated = result.Truncated,
                stdout = result.StdOut,
                stderr = result.StdErr,
                note = result.Error
            });
        }
    }

    private static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error = GitRunner.MaskCredentials(error) }, JsonOptions);

    private static string Ok(object payload) =>
        JsonSerializer.Serialize(new { success = true, data = payload }, JsonOptions);
}
