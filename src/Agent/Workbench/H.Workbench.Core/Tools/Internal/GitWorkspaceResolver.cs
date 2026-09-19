using System.Security.Cryptography;
using System.Text;

namespace H.Workbench.Core.Tools.Internal;

/// <summary>
/// 仓库工作目录解析器：把 repoUrl/目录名统一解析到 WorkDir 内的物理目录，
/// 并强制路径护栏（禁 ..、禁绝对路径逃逸、禁符号链接逃逸）。
/// </summary>
public class GitWorkspaceResolver
{
    private readonly WorkbenchToolOptions _options;

    public GitWorkspaceResolver(Microsoft.Extensions.Options.IOptions<WorkbenchToolOptions> options)
    {
        _options = options.Value;
    }

    public string WorkDir => _options.Git.WorkDir;

    /// <summary>
    /// WorkDir 是否已配置；未配置时 git/工作区工具必须拒绝执行并给出明确错误
    /// </summary>
    public bool TryGetRoot(out string root, out string? error)
    {
        root = "";
        if (string.IsNullOrWhiteSpace(_options.Git.WorkDir))
        {
            error = "未配置 Workbench:Git:WorkDir（服务端仓库工作目录），git 与工作区工具不可用";
            return false;
        }

        try
        {
            root = Path.GetFullPath(_options.Git.WorkDir);
            Directory.CreateDirectory(root);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = $"工作目录 {_options.Git.WorkDir} 不可用: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 由仓库地址派生目录名：host-org-repo + 短 hash 防碰撞
    /// </summary>
    public static string DeriveDirName(string repoUrl)
    {
        var url = repoUrl.Trim();
        string host = "", path = url;

        if (url.Contains("://"))
        {
            var idx = url.IndexOf("://", StringComparison.Ordinal);
            var rest = url[(idx + 3)..];
            var slash = rest.IndexOf('/');
            host = slash >= 0 ? rest[..slash] : rest;
            path = slash >= 0 ? rest[(slash + 1)..] : "";
        }
        else if (url.Contains('@') && url.Contains(':'))
        {
            // git@host:org/repo.git
            var at = url.IndexOf('@');
            var colon = url.IndexOf(':', at);
            host = colon > at ? url[(at + 1)..colon] : "";
            path = colon >= 0 ? url[(colon + 1)..] : url;
        }

        if (host.EndsWith(".local")) host = host[..^6];
        if (host.Contains(':')) host = host.Split(':')[0];

        var segments = new List<string>();
        var hostPart = Sanitize(host);
        if (!string.IsNullOrEmpty(hostPart)) segments.Add(hostPart);
        foreach (var seg in path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var s = Sanitize(seg.EndsWith(".git", StringComparison.OrdinalIgnoreCase) ? seg[..^4] : seg);
            if (!string.IsNullOrEmpty(s)) segments.Add(s);
        }

        var name = string.Join("-", segments);
        if (name.Length > 60) name = name[..60].TrimEnd('-');
        if (string.IsNullOrEmpty(name)) name = "repo";

        var hash = Sha256_6(url.ToLowerInvariant());
        return $"{name}-{hash}";

        static string Sanitize(string input)
        {
            var sb = new StringBuilder();
            foreach (var c in input)
            {
                sb.Append(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-' ? c : '-');
            }
            return sb.ToString().Trim('-');
        }
    }

    private static string Sha256_6(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes)[..8].ToLowerInvariant();
    }

    /// <summary>
    /// 把 "repo 参数"（URL / 目录名 / 目录名子路径）解析为 WorkDir 内的绝对路径。
    /// requireExisting=true 时目录必须已存在。
    /// </summary>
    public bool TryResolveRepo(string repo, bool requireExisting, out string fullPath, out string? error)
    {
        fullPath = "";
        error = null;

        if (!TryGetRoot(out var root, out error)) return false;
        if (string.IsNullOrWhiteSpace(repo)) { error = "repo 参数不能为空"; return false; }

        var candidate = repo.Trim();

        // URL → 派生目录名
        if (candidate.Contains("://") || (candidate.Contains('@') && candidate.Contains(':')) ||
            candidate.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            candidate = DeriveDirName(candidate);
        }

        // 去 .git 尾巴（用户可能传 "applab.git"）
        if (candidate.EndsWith(".git", StringComparison.OrdinalIgnoreCase) && !candidate.Contains('/'))
        {
            candidate = candidate[..^4];
        }

        var combined = Path.GetFullPath(Path.Combine(root, candidate));
        if (!IsInsideRoot(combined, root))
        {
            error = $"拒绝访问工作目录之外的路径: {repo}";
            return false;
        }

        if (requireExisting)
        {
            if (!Directory.Exists(combined))
            {
                error = $"仓库 {candidate} 尚未克隆到工作目录（可先调用 GitCloneAsync 或 GitListReposAsync 查看）";
                return false;
            }
            if (IsReparsePoint(combined))
            {
                error = "仓库目录是符号链接，出于安全拒绝访问";
                return false;
            }
        }

        fullPath = combined;
        return true;
    }

    /// <summary>
    /// 把仓库内相对路径解析为绝对路径（限定在仓库目录内，拒 .. 与符号链接逃逸）
    /// </summary>
    public static bool TryResolveInRepo(string repoDir, string relativePath, out string fullPath, out string? error)
    {
        fullPath = "";
        error = null;

        var rel = (relativePath ?? ".").Replace('\\', '/').Trim();
        if (rel.StartsWith('/') || Path.IsPathRooted(rel))
        {
            error = "拒绝绝对路径，只接受仓库内相对路径";
            return false;
        }

        var combined = Path.GetFullPath(Path.Combine(repoDir, rel));
        var normalizedRepo = Path.GetFullPath(repoDir);
        if (!combined.StartsWith(normalizedRepo + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(combined, normalizedRepo, StringComparison.OrdinalIgnoreCase))
        {
            error = $"拒绝越出仓库目录的路径: {relativePath}";
            return false;
        }

        if (IsInsideGitDir(combined, normalizedRepo))
        {
            error = "拒绝访问 .git 目录内部";
            return false;
        }

        if (File.Exists(combined) && IsReparsePoint(combined))
        {
            error = "目标是符号链接，出于安全拒绝访问";
            fullPath = "";
            return false;
        }

        fullPath = combined;
        return true;
    }

    public static bool IsInsideGitDir(string path, string repoDir)
    {
        var rel = Path.GetRelativePath(repoDir, path);
        return rel.StartsWith(".git", StringComparison.OrdinalIgnoreCase)
               && (rel.Length == 4 || rel[4] is '\\' or '/');
    }

    private static bool IsInsideRoot(string path, string root)
    {
        var normalizedRoot = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        return path.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsReparsePoint(string path)
    {
        try
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
        }
        catch
        {
            return false;
        }
    }
}
