using H.Workbench.Core.Tools.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 工作区文件工具 - 在 git 工作目录内的仓库副本中列出/读取/写入/搜索文件。
/// 所有路径被强制限定在工作目录内的仓库目录中；本轮不提供删除能力。
/// </summary>
public class WorkspaceFileTool
{
    private const long MaxReadBytes = 512 * 1024;
    private static readonly string[] SkipDirectories = { ".git", "bin", "obj", "node_modules", "dist", ".vs" };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly GitWorkspaceResolver _resolver;
    private readonly ILogger<WorkspaceFileTool> _logger;

    public WorkspaceFileTool(IOptions<WorkbenchToolOptions> options, ILogger<WorkspaceFileTool> logger)
    {
        _resolver = new GitWorkspaceResolver(options);
        _logger = logger;
    }

    [Description("列出仓库目录内的文件。参数：repo, subPath, recursive, maxEntries。")]
    public Task<string> WorkspaceListFilesAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        [Description("仓库内的子目录相对路径，默认仓库根")] string subPath = ".",
        [Description("是否递归子目录")] bool recursive = true,
        [Description("最大返回条目数")] int maxEntries = 200,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var repoDir, out var error)) return Task.FromResult(Fail(error!));
        if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, subPath, out var target, out error)) return Task.FromResult(Fail(error!));
        if (!Directory.Exists(target)) return Task.FromResult(Fail($"目录不存在: {subPath}"));

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        var files = new List<string>();
        foreach (var file in Directory.EnumerateFiles(target, "*", option))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsSkipped(file, target!)) continue;
            files.Add(Path.GetRelativePath(repoDir!, file).Replace('\\', '/'));
            if (files.Count >= Math.Clamp(maxEntries, 1, 2000)) break;
        }

        files.Sort(StringComparer.Ordinal);
        return Task.FromResult(Ok(new
        {
            repo = Path.GetFileName(repoDir!),
            subPath,
            count = files.Count,
            truncated = files.Count >= maxEntries,
            files
        }));
    }

    [Description("读取仓库内文本文件（带行号，支持分页）。参数：repo, relativePath, startLine, maxLines。")]
    public async Task<string> WorkspaceReadFileAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        [Description("仓库内相对路径，如 src/App.razor")] string relativePath,
        [Description("起始行号（1 起）")] int startLine = 1,
        [Description("最多读取行数")] int maxLines = 400,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var repoDir, out var error)) return Fail(error!);
        if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, relativePath, out var file, out error)) return Fail(error!);
        if (!File.Exists(file)) return Fail($"文件不存在: {relativePath}");

        var info = new FileInfo(file!);
        if (info.Length > MaxReadBytes)
            return Fail($"文件过大（{info.Length} 字节，上限 {MaxReadBytes}），请用 startLine/maxLines 或先缩小范围");

        string[] lines;
        try
        {
            lines = await File.ReadAllLinesAsync(file!, cancellationToken);
        }
        catch (IOException)
        {
            return Fail($"文件被占用无法读取: {relativePath}");
        }

        var from = Math.Clamp(startLine - 1, 0, lines.Length);
        var take = Math.Clamp(maxLines, 1, 2000);
        var selected = lines.Skip(from).Take(take)
            .Select((line, i) => $"{from + i + 1}: {line}");

        return Ok(new
        {
            repo = Path.GetFileName(repoDir!),
            file = relativePath,
            totalLines = lines.Length,
            from,
            content = string.Join('\n', selected)
        });
    }

    [Description("写入/覆盖仓库内文本文件（相对路径）。参数：repo, relativePath, content, createDirectories。")]
    public async Task<string> WorkspaceWriteFileAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        [Description("仓库内相对路径")] string relativePath,
        [Description("要写入的完整文本内容")] string content,
        [Description("父目录不存在时是否自动创建")] bool createDirectories = true,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var repoDir, out var error)) return Fail(error!);
        if (string.IsNullOrWhiteSpace(relativePath)) return Fail("relativePath 不能为空");
        if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, relativePath, out var file, out error)) return Fail(error!);
        if (GitWorkspaceResolver.IsInsideGitDir(file!, repoDir!)) return Fail("拒绝写入 .git 目录");

        var existed = File.Exists(file);
        if (!createDirectories && !Directory.Exists(Path.GetDirectoryName(file)))
            return Fail($"父目录不存在且 createDirectories=false: {relativePath}");

        Directory.CreateDirectory(Path.GetDirectoryName(file!)!);
        await File.WriteAllTextAsync(file!, content ?? string.Empty, cancellationToken);
        _logger.LogInformation("工作区写入文件: {File}", GitRunner.MaskCredentials(relativePath));

        return Ok(new
        {
            repo = Path.GetFileName(repoDir!),
            file = relativePath,
            created = !existed,
            bytes = content?.Length ?? 0
        });
    }

    /// <summary>
    /// 精确替换。此前只有整文件覆盖，等于要求模型每次把整个文件重打一遍——
    /// 大文件既烧 token，又会悄悄丢掉它没抄到的部分。
    /// </summary>
    [Description("对仓库内已存在的文本文件做精确替换（按原文匹配，不用行号）。参数：repo, relativePath, edits（JSON 数组，每项 {oldText,newText,replaceAll}）。任一条匹配失败则整笔不写入。")]
    public async Task<string> WorkspaceEditFileAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        [Description("仓库内相对路径")] string relativePath,
        [Description("""替换指令 JSON 数组，如 [{"oldText":"原片段","newText":"新片段","replaceAll":false}]""")] string edits,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var repoDir, out var error)) return Fail(error!);
        if (string.IsNullOrWhiteSpace(relativePath)) return Fail("relativePath 不能为空");
        if (!GitWorkspaceResolver.TryResolveInRepo(repoDir!, relativePath, out var file, out error)) return Fail(error!);
        if (GitWorkspaceResolver.IsInsideGitDir(file!, repoDir!)) return Fail("拒绝改写 .git 目录");
        if (!File.Exists(file)) return Fail($"文件不存在；新增文件请用 WorkspaceWriteFileAsync：{relativePath}");

        List<EditOp>? ops;
        try
        {
            ops = JsonSerializer.Deserialize<List<EditOp>>(edits, EditJsonOptions);
        }
        catch (JsonException ex)
        {
            return Fail($"edits 不是合法 JSON 数组：{ex.Message}");
        }

        if (ops is null or { Count: 0 }) return Fail("edits 不能为空");

        var original = await File.ReadAllTextAsync(file!, cancellationToken);
        var working = original;
        var applied = 0;

        for (var i = 0; i < ops.Count; i++)
        {
            var op = ops[i];
            if (string.IsNullOrEmpty(op.OldText)) return Fail($"第 {i + 1} 条 edit 的 oldText 为空");
            if (op.NewText is null) return Fail($"第 {i + 1} 条 edit 缺少 newText（要删除片段请把 newText 设为空字符串）");

            // 模型给的片段常是 \n，而 Windows 仓库里是 \r\n：按两种换行都试一次，
            // 否则会出现"内容明明在却报未找到"
            var needle = ResolveNewlineVariant(working, op.OldText);
            if (needle is null) return Fail($"第 {i + 1} 条 edit 未找到匹配文本，整笔未写入");

            var matches = CountOccurrences(working, needle);
            if (matches > 1 && !op.ReplaceAll)
            {
                return Fail($"第 {i + 1} 条 edit 匹配到 {matches} 处，需要多带几行上下文使其唯一，或显式设 replaceAll=true；整笔未写入");
            }

            working = op.ReplaceAll
                ? working.Replace(needle, op.NewText)
                : ReplaceFirst(working, needle, op.NewText);
            applied++;
        }

        var repoName = Path.GetFileName(repoDir!);
        if (working == original)
        {
            return Ok(new { repo = repoName, file = relativePath, editsApplied = applied, changed = false, note = "替换后内容与原文件相同，未写入" });
        }

        var diff = LineDiff.Unified(relativePath.Replace('\\', '/'), original, working);
        await File.WriteAllTextAsync(file!, working, cancellationToken);
        _logger.LogInformation("工作区精确编辑: {File}（{Count} 处替换）", GitRunner.MaskCredentials(relativePath), applied);

        return Ok(new
        {
            repo = repoName,
            file = relativePath,
            editsApplied = applied,
            changed = true,
            bytes = working.Length,
            diff
        });
    }

    private sealed record EditOp
    {
        public string? OldText { get; set; }
        public string? NewText { get; set; }
        public bool ReplaceAll { get; set; }
    }

    private static readonly JsonSerializerOptions EditJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private static string? ResolveNewlineVariant(string haystack, string needle)
    {
        if (haystack.Contains(needle)) return needle;

        var lf = needle.Replace("\r\n", "\n");
        if (haystack.Contains(lf)) return lf;

        var crlf = lf.Replace("\n", "\r\n");
        return haystack.Contains(crlf) ? crlf : null;
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }
        return count;
    }

    private static string ReplaceFirst(string text, string value, string replacement)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        return index < 0 ? text : text[..index] + replacement + text[(index + value.Length)..];
    }

    [Description("在仓库内按关键字搜索文件内容（不区分大小写）。参数：repo, keyword, filePattern, maxResults。")]
    public async Task<string> WorkspaceSearchAsync(
        [Description("仓库地址或已克隆的目录名")] string repo,
        [Description("搜索关键字")] string keyword,
        [Description("文件名通配模式，如 *.cs、*.razor，默认 *.cs")] string filePattern = "*.cs",
        [Description("最大命中数")] int maxResults = 50,
        CancellationToken cancellationToken = default)
    {
        if (!ResolveRepoDir(repo, out var repoDir, out var error)) return Fail(error!);
        if (string.IsNullOrWhiteSpace(keyword)) return Fail("keyword 不能为空");

        var limit = Math.Clamp(maxResults, 1, 500);
        var matches = new List<object>();
        var pattern = string.IsNullOrWhiteSpace(filePattern) ? "*" : filePattern;

        foreach (var file in Directory.EnumerateFiles(repoDir!, pattern, SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsSkipped(file, repoDir!)) continue;

            string[] lines;
            try
            {
                var info = new FileInfo(file);
                if (info.Length > MaxReadBytes) continue;
                lines = await File.ReadAllLinesAsync(file, cancellationToken);
            }
            catch
            {
                continue; // 二进制/无权限文件跳过
            }

            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains(keyword, StringComparison.OrdinalIgnoreCase)) continue;
                matches.Add(new
                {
                    file = Path.GetRelativePath(repoDir!, file).Replace('\\', '/'),
                    line = i + 1,
                    text = lines[i].Trim().Length > 200 ? lines[i].Trim()[..200] : lines[i].Trim()
                });
                if (matches.Count >= limit) break;
            }

            if (matches.Count >= limit) break;
        }

        return Ok(new
        {
            repo = Path.GetFileName(repoDir!),
            keyword,
            count = matches.Count,
            truncated = matches.Count >= limit,
            matches
        });
    }

    private bool ResolveRepoDir(string repo, out string? dir, out string? error)
    {
        var ok = _resolver.TryResolveRepo(repo, requireExisting: true, out var full, out error);
        dir = ok ? full : null;
        return ok;
    }

    private static bool IsSkipped(string file, string root)
    {
        var relative = Path.GetRelativePath(root, file);
        return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Any(seg => SkipDirectories.Contains(seg, StringComparer.OrdinalIgnoreCase));
    }

    private static string Fail(string error) =>
        JsonSerializer.Serialize(new { success = false, error = GitRunner.MaskCredentials(error) }, JsonOptions);

    private static string Ok(object payload) =>
        JsonSerializer.Serialize(new { success = true, data = payload }, JsonOptions);
}
