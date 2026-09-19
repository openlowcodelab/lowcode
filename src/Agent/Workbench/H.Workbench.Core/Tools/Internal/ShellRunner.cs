using System.Diagnostics;
using System.Text;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// shell 子进程执行器（workspace_shell 专用）。镜像 GitRunner 的防死锁写法：
/// 先起后台读流再等退出、超时击杀整棵进程树、凭据掩码。不改 GitRunner 避免 git 路径回归。
/// </summary>
public static class ShellRunner
{
    public static async Task<ShellRunResult> RunAsync(
        string workingDirectory,
        string command,
        bool usePwsh,
        int timeoutSeconds,
        int maxOutputChars,
        CancellationToken ct = default)
    {
        var (fileName, args) = OSPlatformExtensions.ShellCommand(usePwsh, command);

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        // 命令整串作为单个 argument 交给 shell 自身解析，不经二次字符串拼接
        foreach (var arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var truncated = false;

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            if (stdout.Length < maxOutputChars) stdout.AppendLine(e.Data);
            else truncated = true;
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            if (stderr.Length < 4000) stderr.AppendLine(e.Data);
            else truncated = true;
        };

        var startedAt = DateTime.UtcNow;
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds)));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                var reason = ct.IsCancellationRequested ? "已取消" : $"执行超时（{timeoutSeconds}秒）";
                return new ShellRunResult(-1, Flush(stdout, maxOutputChars), Flush(stderr, 4000), truncated,
                    (int)(DateTime.UtcNow - startedAt).TotalMilliseconds, reason);
            }

            return new ShellRunResult(process.ExitCode, Flush(stdout, maxOutputChars), Flush(stderr, 4000),
                truncated, (int)(DateTime.UtcNow - startedAt).TotalMilliseconds, null);
        }
        catch (Exception ex)
        {
            TryKill(process);
            return new ShellRunResult(-1, "", $"进程启动失败: {GitRunner.MaskCredentials(ex.Message)}", false,
                (int)(DateTime.UtcNow - startedAt).TotalMilliseconds, "启动失败");
        }
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch { /* 已退出或无权限 */ }
    }

    private static string Flush(StringBuilder sb, int max)
    {
        var text = sb.ToString().TrimEnd();
        return text.Length > max ? text[..max] + "\n...[输出已截断]" : text;
    }
}

public sealed record ShellRunResult(int ExitCode, string StdOut, string StdErr, bool Truncated, int DurationMs, string? Error);

internal static class OSPlatformExtensions
{
    /// <summary>
    /// Windows 默认 cmd.exe（/d 跳 AutoRun、/s 保留引号语义），可选 pwsh；非 Windows 用 bash。
    /// </summary>
    public static (string FileName, string[] Args) ShellCommand(bool usePwsh, string command)
    {
        if (OperatingSystem.IsWindows())
        {
            return usePwsh
                ? ("pwsh.exe", new[] { "-NoProfile", "-NonInteractive", "-Command", command })
                : ("cmd.exe", new[] { "/d", "/s", "/c", command });
        }

        if (usePwsh && OperatingSystem.IsLinux())
        {
            return ("pwsh", new[] { "-NoProfile", "-NonInteractive", "-Command", command });
        }

        return ("/bin/bash", new[] { "-lc", command });
    }
}
