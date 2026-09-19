using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;

namespace H.Workbench.Core.Tools;

/// <summary>
/// Git 工具 - 在服务端工作目录内克隆/管理 git 仓库（shell 调 git.exe）。
/// 实例由 DI 单例提供；只允许依赖 IOptions/ILogger/单例锁（红线：不得注入 scoped 服务）。
/// </summary>
public class GitTool
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly GitToolOptions _options;
    private readonly GitRunner _runner;
    private readonly GitWorkspaceResolver _resolver;
    private readonly GitWorkspaceLocks _locks;
    private readonly ILogger<GitTool> _logger;

    public GitTool(IOptions<WorkbenchToolOptions> options, GitWorkspaceLocks locks, ILogger<GitTool> logger)
    {
        _options = options.Value.Git;
        _locks = locks;
        _logger = logger;
        _runner = new GitRunner(_options);
        _resolver = new GitWorkspaceResolver(options);
    }

    [Description("克隆 Git 仓库到服务端工作目录。若目标目录已存在则改为拉取更新。参数：repoUrl, branch, dirName, timeoutSeconds。")]
    public async Task<string> GitCloneAsync(
        [Description("HTTPS 仓库地址（含凭据的 URL 会掩码记录），必须来自“可操作的代码仓库”清单")] string repoUrl,
        [Description("要检出的分支；留空使用远端默认分支")] string? branch = null,
        [Description("工作目录下的目录名；留空由仓库地址自动派生")] string? dirName = null,
        [Description("克隆超时（秒），默认 600")] int timeoutSeconds = 600,
        CancellationToken cancellationToken = default)
    {
        if (!_resolver.TryGetRoot(out var root, out var error)) return Fail(error!);

        var targetName = string.IsNullOrWhiteSpace(dirName)
            ? GitWorkspaceResolver.DeriveDirName(repoUrl)
            : SanitizeDirName(dirName!);
        if (string.IsNullOrEmpty(targetName)) return Fail("dirName 非法");

        var targetDir = Path.Combine(root, targetName);
        if (!targetDir.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return Fail("dirName 越出工作目录");

        _logger.LogInformation("Git clone: {RepoUrl} -> {Dir}", GitRunner.MaskCredentials(repoUrl), targetName);

        using (await _locks.AcquireAsync(targetDir, cancellationToken))
        {
            if (Directory.Exists(Path.Combine(targetDir, ".git")))
            {
                var fetch = await _runner.RunAsync(targetDir,
                    new[] { "fetch", "--all", "--prune" }, _options.DefaultTimeoutSeconds, cancellationToken);
                if (!fetch.Success) return Fail($"更新已有克隆失败: {Mask(fetch.Error)}");

                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var co = await _runner.RunAsync(targetDir,
                        new[] { "checkout", branch! }, _options.DefaultTimeoutSeconds, cancellationToken);
                    if (!co.Success) return Fail($"已 fetch 但切换分支 {branch} 失败: {Mask(co.Error)}");
                }

                return Ok(new
                {
                    message = "仓库已存在，执行 fetch 更新",
                    repo = targetName,
                    branch = string.IsNullOrWhiteSpace(branch) ? "(未切换)" : branch
                });
            }

            var cloneArgs = new List<string> { "clone", repoUrl, targetName };
            if (!string.IsNullOrWhiteSpace(branch))
            {
                cloneArgs.Add("--branch");
                cloneArgs.Add(branch!);
            }
            var clone = await _runner.RunAsync(root, cloneArgs,
                Math.Clamp(timeoutSeconds, 10, _options.CloneTimeoutSeconds), cancellationToken);
            if (!clone.Success)
                return Fail($"克隆失败: {Mask(string.Join(' ', clone.Error, clone.Output))}");

            return Ok(new
            {
                message = "克隆成功",
                repo = targetName,
                workingDirectory = targetDir,
                branch = string.IsNullOrWhiteSpace(branch) ? "(远端默认)" : branch
            });
        }
    }

    [Description("查看仓库状态（当前分支、领先/落后、变更文件）。参数：repo（仓库地址或工作目录中的目录名）。")]
    public async Task<string> GitStatusAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        var result = await _runner.RunAsync(dir!, new[] { "status", "--porcelain=v1", "-b" },
            _options.DefaultTimeoutSeconds, cancellationToken);
        if (!result.Success) return Fail(Mask(result.Error));

        var lines = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        return Ok(new { repo = Path.GetFileName(dir!), head = lines.FirstOrDefault(), changes = lines.Skip(1).ToList() });
    }

    [Description("切换或新建分支。参数：repo, branch, createNew。")]
    public async Task<string> GitCheckoutAsync(
        [Description("仓库地址或目录名")] string repo,
        [Description("分支名")] string branch,
        [Description("true 时新建分支，false 时切换到已有本地/远端分支")] bool createNew = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(branch)) return Fail("branch 不能为空");
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        using (await _locks.AcquireAsync(dir!, cancellationToken))
        {
            if (createNew)
            {
                var create = await _runner.RunAsync(dir!, new[] { "checkout", "-b", branch },
                    _options.DefaultTimeoutSeconds, cancellationToken);
                return create.Success
                    ? Ok(new { message = $"已新建并切换到分支 {branch}", repo = Path.GetFileName(dir!) })
                    : Fail($"新建分支失败: {Mask(create.Error)}");
            }

            await _runner.RunAsync(dir!, new[] { "fetch", "origin" }, _options.DefaultTimeoutSeconds, cancellationToken);

            var checkout = await _runner.RunAsync(dir!, new[] { "checkout", branch },
                _options.DefaultTimeoutSeconds, cancellationToken);
            if (checkout.Success)
                return Ok(new { message = $"已切换到分支 {branch}", repo = Path.GetFileName(dir!) });

            var track = await _runner.RunAsync(dir!, new[] { "checkout", "-b", branch, $"origin/{branch}" },
                _options.DefaultTimeoutSeconds, cancellationToken);
            return track.Success
                ? Ok(new { message = $"已基于 origin/{branch} 检出分支 {branch}", repo = Path.GetFileName(dir!) })
                : Fail($"切换分支失败: {Mask(checkout.Error)}");
        }
    }

    [Description("拉取远端更新（--ff-only）。参数：repo, branch（留空用当前分支）。")]
    public async Task<string> GitPullAsync(
        [Description("仓库地址或目录名")] string repo,
        [Description("要拉取的分支；留空使用当前分支")] string? branch = null,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        using (await _locks.AcquireAsync(dir!, cancellationToken))
        {
            var args = new List<string> { "pull", "--ff-only" };
            if (!string.IsNullOrWhiteSpace(branch))
            {
                args.Add("origin");
                args.Add(branch!);
            }
            var result = await _runner.RunAsync(dir!, args, _options.CloneTimeoutSeconds, cancellationToken);
            return result.Success
                ? Ok(new { message = "拉取完成", repo = Path.GetFileName(dir!), output = Mask(result.Output) })
                : Fail($"拉取失败: {Mask(result.Error)}");
        }
    }

    [Description("提交工作区全部变更（git add -A + commit），可选推送远端。无变更时不产生提交。参数：repo, message, branch, push。")]
    public async Task<string> GitCommitPushAsync(
        [Description("仓库地址或目录名")] string repo,
        [Description("提交信息")] string message,
        [Description("推送到的分支；留空推送当前分支")] string? branch = null,
        [Description("是否在提交后推送到远端")] bool push = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message)) return Fail("message 不能为空");
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        using (await _locks.AcquireAsync(dir!, cancellationToken))
        {
            var status = await _runner.RunAsync(dir!, new[] { "status", "--porcelain" },
                _options.DefaultTimeoutSeconds, cancellationToken);
            if (!status.Success) return Fail(Mask(status.Error));
            if (string.IsNullOrWhiteSpace(status.Output))
                return Ok(new { message = "工作区无变更，未创建提交", repo = Path.GetFileName(dir!) });

            var add = await _runner.RunAsync(dir!, new[] { "add", "-A" },
                _options.DefaultTimeoutSeconds, cancellationToken);
            if (!add.Success) return Fail($"git add 失败: {Mask(add.Error)}");

            var commit = await _runner.RunAsync(dir!, new[] { "commit", "-m", message },
                _options.DefaultTimeoutSeconds, cancellationToken);
            if (!commit.Success) return Fail($"git commit 失败: {Mask(commit.Error)}");

            var head = await _runner.RunAsync(dir!, new[] { "rev-parse", "--short", "HEAD" },
                _options.DefaultTimeoutSeconds, cancellationToken);

            string? pushResult = null;
            if (push)
            {
                var pushArgs = new List<string> { "push", "-u", "origin" };
                if (!string.IsNullOrWhiteSpace(branch)) pushArgs.Add(branch!); else pushArgs.Add("HEAD");
                var pushed = await _runner.RunAsync(dir!, pushArgs, _options.CloneTimeoutSeconds, cancellationToken);
                if (!pushed.Success) return Fail($"提交成功（{head.Output}）但推送失败: {Mask(pushed.Error)}");
                pushResult = "已推送";
            }

            return Ok(new
            {
                message = pushResult ?? "已提交（未推送）",
                repo = Path.GetFileName(dir!),
                commit = head.Output,
                changes = status.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length
            });
        }
    }

    [Description("查看提交历史。参数：repo, limit。")]
    public async Task<string> GitLogAsync(
        [Description("仓库地址或目录名")] string repo,
        [Description("返回条数，默认 20")] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        var result = await _runner.RunAsync(dir!,
            new[] { "log", "--oneline", $"-{Math.Clamp(limit, 1, 200)}" },
            _options.DefaultTimeoutSeconds, cancellationToken);
        return result.Success
            ? Ok(new { repo = Path.GetFileName(dir!), commits = result.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries) })
            : Fail(Mask(result.Error));
    }

    [Description("推送当前分支（或指定分支）到远端。参数：repo, branch。")]
    public async Task<string> GitPushAsync(
        [Description("仓库地址或目录名")] string repo,
        [Description("要推送的分支；留空推送当前分支")] string? branch = null,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var dir, out var error)) return Fail(error!);

        using (await _locks.AcquireAsync(dir!, cancellationToken))
        {
            var args = new List<string> { "push", "-u", "origin" };
            if (!string.IsNullOrWhiteSpace(branch)) args.Add(branch!); else args.Add("HEAD");
            var result = await _runner.RunAsync(dir!, args, _options.CloneTimeoutSeconds, cancellationToken);
            return result.Success
                ? Ok(new { message = "推送完成", repo = Path.GetFileName(dir!), output = Mask(result.Output) })
                : Fail($"推送失败: {Mask(result.Error)}");
        }
    }

    [Description("列出服务端工作目录下已克隆的仓库（目录名、当前分支、远端地址）。")]
    public async Task<string> GitListReposAsync(CancellationToken cancellationToken = default)
    {
        if (!_resolver.TryGetRoot(out var root, out var error)) return Fail(error!);

        var repos = new List<object>();
        foreach (var dir in Directory.EnumerateDirectories(root))
        {
            if (!Directory.Exists(Path.Combine(dir, ".git"))) continue;

            var branch = await _runner.RunAsync(dir, new[] { "rev-parse", "--abbrev-ref", "HEAD" },
                _options.DefaultTimeoutSeconds, cancellationToken);
            var remote = await _runner.RunAsync(dir, new[] { "remote", "get-url", "origin" },
                _options.DefaultTimeoutSeconds, cancellationToken);

            repos.Add(new
            {
                repo = Path.GetFileName(dir),
                branch = branch.Success ? branch.Output : "?",
                remoteUrl = GitRunner.MaskCredentials(remote.Success ? remote.Output : "?")
            });
        }

        return Ok(new { workDir = root, count = repos.Count, repos });
    }

    private bool ResolveRepoDir(string repo, out string? dir, out string? error)
    {
        var ok = _resolver.TryResolveRepo(repo, requireExisting: true, out var full, out error);
        dir = ok ? full : null;
        return ok;
    }

    private static string SanitizeDirName(string name)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var c in name.Trim().TrimEnd('/', '\\'))
        {
            sb.Append(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' or '/' ? c : '-');
        }
        var result = sb.ToString().Trim('/', '\\');
        return result.Contains("..") ? "" : result;
    }

    private static string Mask(string? text) => GitRunner.MaskCredentials(text);

    private static string Ok(object payload) =>
        JsonSerializer.Serialize(new { success = true, @data = payload }, JsonOptions);

    private static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error = MaskCredentialsStatic(error) }, JsonOptions);

    private static string MaskCredentialsStatic(string error) => GitRunner.MaskCredentials(error);
}
