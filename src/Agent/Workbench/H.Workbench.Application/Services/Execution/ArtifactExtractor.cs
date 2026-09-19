using System.Text.Json;
using System.Text.RegularExpressions;
using H.Workbench.EntityFrameworkCore;

namespace H.Workbench.Application.Services.Execution;

/// <summary>
/// 从工具返回的结构化 JSON（{success,data}）派生产物行——纯函数、零依赖。
/// 工具侧因此不需要任何持久化能力（单例红线）；识别不了的工具返回空列表。
/// </summary>
public static class ArtifactExtractor
{
    public static List<ArtifactEntity> TryExtract(string? toolName, string? argumentsJson, string? resultJson)
    {
        var artifacts = new List<ArtifactEntity>();
        if (string.IsNullOrEmpty(toolName) || string.IsNullOrWhiteSpace(resultJson))
        {
            return artifacts;
        }

        try
        {
            using var doc = JsonDocument.Parse(resultJson);
            var root = doc.RootElement;
            if (!root.TryGetProperty("success", out var successProp) || successProp.ValueKind != JsonValueKind.True)
            {
                // "提交成功（hash）但推送失败"仍是一条值得验收的产物
                if (toolName == "GitCommitPushAsync" &&
                    root.TryGetProperty("error", out var errProp) &&
                    errProp.GetString() is { } err && err.Contains("提交成功"))
                {
                    var m = Regex.Match(err, @"提交成功（(?<hash>[^）]+)）");
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "GitCommit",
                        Title = "提交成功但推送失败",
                        Repo = GetArg(argumentsJson, "repo"),
                        Branch = GetArg(argumentsJson, "branch"),
                        CommitHash = m.Success ? m.Groups["hash"].Value : null,
                        PushResult = "PushFailed",
                        Success = false
                    });
                }
                return artifacts;
            }

            var data = root.TryGetProperty("data", out var d) ? d : default;
            if (data.ValueKind != JsonValueKind.Object)
            {
                return artifacts;
            }

            switch (toolName)
            {
                case "GitCommitPushAsync":
                {
                    var repo = GetString(data, "repo");
                    var commit = GetString(data, "commit");
                    var changes = data.TryGetProperty("changes", out var c) && c.TryGetInt32(out var cv) ? cv : 0;
                    var message = GetString(data, "message") ?? "";
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "GitCommit",
                        Title = changes > 0 ? $"提交 {changes} 处变更 → {commit}" : message,
                        Repo = repo,
                        Branch = GetArg(argumentsJson, "branch"),
                        CommitHash = commit,
                        PushResult = message.Contains("已推送") ? "Pushed" : "NotPushed",
                        Success = true,
                        Payload = changes > 0 ? $"{{\"changes\":{changes}}}" : null
                    });
                    break;
                }
                case "GitPushAsync":
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "GitPush",
                        Title = $"推送仓库 {GetString(data, "repo")}",
                        Repo = GetString(data, "repo"),
                        Branch = GetArg(argumentsJson, "branch"),
                        PushResult = "Pushed",
                        Success = true
                    });
                    break;
                case "GitCloneAsync":
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Clone",
                        Title = GetString(data, "message") ?? "克隆仓库",
                        Repo = GetString(data, "repo"),
                        Branch = GetString(data, "branch"),
                        Success = true
                    });
                    break;
                case "WorkspaceWriteFileAsync":
                {
                    var file = GetString(data, "file");
                    var created = data.TryGetProperty("created", out var cr) && cr.ValueKind == JsonValueKind.True;
                    var bytes = data.TryGetProperty("bytes", out var b) && b.TryGetInt32(out var bv) ? bv : 0;
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "FileChange",
                        Title = $"{(created ? "新建" : "修改")}文件 {file}",
                        Repo = GetString(data, "repo"),
                        FilePath = file,
                        ChangeType = created ? "Created" : "Modified",
                        Success = true,
                        Payload = $"{{\"bytes\":{bytes}}}"
                    });
                    break;
                }
                case "WorkspaceRunShellAsync":
                {
                    var exitCode = data.TryGetProperty("exitCode", out var ec) && ec.TryGetInt32(out var ev) ? ev : (int?)null;
                    var command = GetArg(argumentsJson, "command");
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "ShellCommand",
                        Title = $"执行命令 {(command?.Length > 120 ? command[..120] + "…" : command)}",
                        Repo = GetString(data, "repo") ?? GetArg(argumentsJson, "repo"),
                        Success = exitCode == 0,
                        Payload = exitCode.HasValue ? $"{{\"exitCode\":{exitCode.Value}}}" : null
                    });
                    break;
                }
            }
        }
        catch (JsonException)
        {
            // 非 JSON 返回（旧文本工具）不产产物
        }

        return artifacts;
    }

    private static string? GetString(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    private static string? GetArg(string? argumentsJson, string name)
    {
        if (string.IsNullOrWhiteSpace(argumentsJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            return GetString(doc.RootElement, name);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
