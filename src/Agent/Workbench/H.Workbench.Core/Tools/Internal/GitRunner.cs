using System.Diagnostics;
using System.Text;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// git 子进程执行器。通过 ArgumentList 逐参传递，杜绝命令注入；
/// 输出/错误在等待前即开始读取，避免管道缓冲死锁。
/// </summary>
public class GitRunner
{
    private readonly GitToolOptions _options;

    public GitRunner(GitToolOptions options)
    {
        _options = options;
    }

    public async Task<GitRunResult> RunAsync(string workingDirectory, IEnumerable<string> arguments,
        int timeoutSeconds, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = _options.ExecutablePath,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        psi.ArgumentList.Add("-c"); psi.ArgumentList.Add("core.quotepath=false");
        psi.ArgumentList.Add("-c"); psi.ArgumentList.Add("color.ui=false");
        psi.ArgumentList.Add("-c"); psi.ArgumentList.Add($"user.name={_options.AuthorName}");
        psi.ArgumentList.Add("-c"); psi.ArgumentList.Add($"user.email={_options.AuthorEmail}");
        foreach (var arg in arguments)
        {
            psi.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

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
                return GitRunResult.Failure(-1, "", $"git 命令执行超时（{timeoutSeconds}秒）");
            }

            var output = stdout.ToString().TrimEnd();
            var error = stderr.ToString().TrimEnd();
            return process.ExitCode == 0
                ? GitRunResult.Ok(output, error)
                : GitRunResult.Failure(process.ExitCode, output, error);
        }
        catch (Exception ex)
        {
            TryKill(process);
            return GitRunResult.Failure(-1, "", $"git 进程启动失败: {ex.Message}");
        }
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch { /* 进程已退出或无权限，忽略 */ }
    }

    /// <summary>
    /// 掩码 URL 中嵌入的凭据（https://user:token@host → https://user:***@host）
    /// </summary>
    public static string MaskCredentials(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        return System.Text.RegularExpressions.Regex.Replace(
            text, "://([^/@:\\s]+):([^/@\\s]+)@", "://$1:***@");
    }
}

public sealed class GitRunResult
{
    public bool Success { get; private init; }
    public int ExitCode { get; private init; }
    public string Output { get; private init; } = "";
    public string Error { get; private init; } = "";

    public static GitRunResult Ok(string output, string error) =>
        new() { Success = true, ExitCode = 0, Output = output, Error = error };

    public static GitRunResult Failure(int exitCode, string output, string error) =>
        new() { Success = false, ExitCode = exitCode, Output = output, Error = error };
}
