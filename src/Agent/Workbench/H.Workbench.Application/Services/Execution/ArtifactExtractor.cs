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
    private static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

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
                case "WorkspaceEditFileAsync":
                {
                    var file = GetString(data, "file");
                    var changed = data.TryGetProperty("changed", out var ch) && ch.ValueKind == JsonValueKind.True;
                    var applied = data.TryGetProperty("editsApplied", out var ap) && ap.TryGetInt32(out var apv) ? apv : 0;
                    var diff = GetString(data, "diff");

                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "FileChange",
                        Title = changed ? $"编辑 {file}（{applied} 处替换）" : $"编辑未产生变更 {file}",
                        Repo = GetString(data, "repo"),
                        FilePath = file,
                        ChangeType = "Modified",
                        Success = true,
                        // diff 必须走序列化转义：里面全是引号与换行，插值拼字符串会写出非法 JSON。
                        // 关掉宽松转义以外的默认行为，否则 '+' 变成 \u002B 没法读
                        Payload = JsonSerializer.Serialize(new
                        {
                            editsApplied = applied,
                            changed,
                            diff = diff is { Length: > 3000 } ? diff[..3000] + "\n…[已截断]" : diff
                        }, PayloadJsonOptions)
                    });
                    break;
                }
                case "WorkspaceRunTestsAsync":
                {
                    int Num(string key) => data.TryGetProperty(key, out var v) && v.TryGetInt32(out var nv) ? nv : 0;
                    var passed = Num("passed");
                    var failed = Num("failed");
                    var skipped = Num("skipped");
                    var exitCode = data.TryGetProperty("exitCode", out var ec) && ec.TryGetInt32(out var ev) ? ev : (int?)null;

                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "TestRun",
                        Title = $"测试 {passed} 通过 / {failed} 失败 / {skipped} 跳过",
                        Repo = GetString(data, "repo"),
                        Success = failed == 0 && exitCode == 0,
                        Payload = JsonSerializer.Serialize(new { passed, failed, skipped, exitCode })
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
                case "OfficeWriteDocumentAsync":
                {
                    var file = GetString(data, "file");
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Document",
                        Title = $"生成文档 {file}",
                        Repo = GetArg(argumentsJson, "repo"),
                        FilePath = file,
                        ChangeType = "Created",
                        Success = true,
                        Payload = JsonSerializer.Serialize(new
                        {
                            blocks = Int(data, "blocks"),
                            paragraphs = Int(data, "paragraphs"),
                            tables = Int(data, "tables"),
                            sizeBytes = Int(data, "sizeBytes")
                        }, PayloadJsonOptions)
                    });
                    break;
                }
                case "SheetWriteAsync":
                {
                    var file = GetString(data, "file");
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Table",
                        Title = $"生成表格 {file}",
                        Repo = GetArg(argumentsJson, "repo"),
                        FilePath = file,
                        ChangeType = "Created",
                        Success = true,
                        Payload = JsonSerializer.Serialize(new
                        {
                            format = GetString(data, "format"),
                            sheets = ArrayLength(data, "sheets"),
                            rows = Int(data, "rows"),
                            sizeBytes = Int(data, "sizeBytes")
                        }, PayloadJsonOptions)
                    });
                    break;
                }
                case "BrowserScreenshotAsync":
                case "ScreenCaptureAsync":
                {
                    var file = GetString(data, "file");
                    var wholeScreen = toolName == "ScreenCaptureAsync";
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Screenshot",
                        Title = wholeScreen ? $"屏幕截图 {file}" : $"页面截图 {file}",
                        FilePath = file,
                        ChangeType = "Created",
                        Success = true,
                        Payload = JsonSerializer.Serialize(new
                        {
                            sizeBytes = Int(data, "sizeBytes"),
                            width = Int(data, "width"),
                            height = Int(data, "height"),
                            url = GetString(data, "url")
                        }, PayloadJsonOptions)
                    });
                    break;
                }
                case "NotifySendAsync":
                {
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Message",
                        Title = $"推送消息到 {GetString(data, "channel")}",
                        Success = true,
                        Payload = JsonSerializer.Serialize(new
                        {
                            channel = GetString(data, "channel"),
                            kind = GetString(data, "kind"),
                            title = GetString(data, "title"),
                            contentChars = Int(data, "contentChars")
                        }, PayloadJsonOptions)
                    });
                    break;
                }
                case "EmailSendAsync":
                {
                    artifacts.Add(new ArtifactEntity
                    {
                        Kind = "Email",
                        Title = $"发送邮件《{GetString(data, "subject")}》",
                        Success = true,
                        Payload = JsonSerializer.Serialize(new
                        {
                            channel = GetString(data, "channel"),
                            recipients = Int(data, "recipients"),
                            ccCount = Int(data, "ccCount"),
                            isHtml = data.TryGetProperty("isHtml", out var h) && h.ValueKind == JsonValueKind.True
                        }, PayloadJsonOptions)
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

    private static int Int(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var v) && v.TryGetInt32(out var value)
            ? value
            : 0;

    private static int ArrayLength(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Array
            ? v.GetArrayLength()
            : 0;

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
