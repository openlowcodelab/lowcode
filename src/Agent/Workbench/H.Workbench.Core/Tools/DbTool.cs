using H.Workbench.Core.Tools.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Data;
using System.Text.Json;

namespace H.Workbench.Core.Tools;

/// <summary>
/// 数据库访问工具。
/// 红线：模型**不得**提供连接串——只能引用 appsettings 里登记过的数据源名（Workbench:DataSources）。
/// 此前 connectionString 是工具入参，等于让模型连任意它能编造出来的库，且连接串会进轨迹与提示词。
/// 实例类：由 ToolRegistry 经 ActivatorUtilities 创建（只依赖单例 IOptions）。
/// </summary>
public class DbTool
{
    /// <summary>
    /// 单次查询回传上限：不设限时结果会被 ToolExecutor 按字符截断成非法 JSON
    /// </summary>
    private const int MaxRows = 200;

    private readonly WorkbenchToolOptions _options;

    public DbTool(IOptions<WorkbenchToolOptions> options)
    {
        _options = options.Value;
    }

    private sealed record ResolvedSource(string Name, string ConnectionString, bool ReadOnly);

    private bool TryResolve(string? dataSource, out ResolvedSource? source, out string? error)
    {
        source = null;
        error = null;

        var registered = _options.DataSources ?? new List<DataSourceOptions>();
        if (registered.Count == 0)
        {
            error = "未配置任何数据源（Workbench:DataSources 为空），数据库工具不可用";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            error = $"必须指定已登记的数据源名，可选：{string.Join(", ", registered.Select(x => x.Name))}";
            return false;
        }

        var hit = registered.FirstOrDefault(x => string.Equals(x.Name, dataSource.Trim(), StringComparison.OrdinalIgnoreCase));
        if (hit is null)
        {
            error = $"数据源 {dataSource} 未登记，可选：{string.Join(", ", registered.Select(x => x.Name))}";
            return false;
        }

        source = new ResolvedSource(hit.Name, hit.ConnectionString, hit.ReadOnly);
        return true;
    }

    [Description("列出可用数据源（名称与是否只读）。数据库工具的 dataSource 参数只能取这些名称。")]
    public Task<string> ListDataSourcesAsync() =>
        Task.FromResult(ToolEnvelope.Ok((_options.DataSources ?? new List<DataSourceOptions>())
            .Select(x => new { name = x.Name, readOnly = x.ReadOnly })));

    [Description("执行 SQL 查询。参数：dataSource（已登记的数据源名）, sql, parameters, commandTimeout。")]
    public async Task<string> ExecuteQueryAsync(
        [Description("已登记的数据源名称，可用 DbListDataSourcesAsync 查询")] string dataSource,
        [Description("SQL 查询语句")] string sql,
        [Description("查询参数（JSON 格式），如 {\"@name\": \"value\"}，可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteQueryAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        if (!TryResolve(dataSource, out var resolved, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            var paramDict = ParseParameters(parameters);
            var results = new List<Dictionary<string, object>>();

            using var connection = new SqlConnection(resolved!.ConnectionString);
            using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = Math.Max(1, commandTimeout),
                CommandType = CommandType.Text
            };

            // 添加参数
            if (paramDict != null)
            {
                foreach (var kv in paramDict)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                }
            }

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            var extraRows = 0;
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (results.Count >= MaxRows)
                {
                    extraRows++;
                    continue;
                }

                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }

            return ToolEnvelope.Ok(new
            {
                rowCount = results.Count,
                data = results,
                note = extraRows > 0
                    ? $"结果超过 {MaxRows} 行，已截断，还有 {extraRows} 行未返回（请改用 TOP/聚合收窄查询）"
                    : null
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"查询失败: {ex.Message}");
        }
    }

    [Description("执行 SQL 命令（INSERT/UPDATE/DELETE）。参数：dataSource（已登记的数据源名）, sql, parameters, commandTimeout。")]
    public async Task<string> ExecuteCommandAsync(
        [Description("已登记的数据源名称")] string dataSource,
        [Description("SQL 命令语句")] string sql,
        [Description("命令参数（JSON 格式），可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteCommandAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        if (!TryResolve(dataSource, out var resolved, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        if (resolved!.ReadOnly)
        {
            return ToolEnvelope.Fail($"数据源 {resolved.Name} 登记为只读，拒绝执行写操作");
        }

        try
        {
            var paramDict = ParseParameters(parameters);

            using var connection = new SqlConnection(resolved!.ConnectionString);
            using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = Math.Max(1, commandTimeout),
                CommandType = CommandType.Text
            };

            if (paramDict != null)
            {
                foreach (var kv in paramDict)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                }
            }

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return ToolEnvelope.Ok(new { affectedRows });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"执行失败: {ex.Message}");
        }
    }

    [Description("执行标量查询（返回单个值）。参数：dataSource（已登记的数据源名）, sql, parameters, commandTimeout。")]
    public async Task<string> ExecuteScalarAsync(
        [Description("已登记的数据源名称")] string dataSource,
        [Description("SQL 查询语句")] string sql,
        [Description("查询参数（JSON 格式），可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteScalarAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        if (!TryResolve(dataSource, out var resolved, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            var paramDict = ParseParameters(parameters);

            using var connection = new SqlConnection(resolved!.ConnectionString);
            using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = Math.Max(1, commandTimeout),
                CommandType = CommandType.Text
            };

            if (paramDict != null)
            {
                foreach (var kv in paramDict)
                {
                    command.Parameters.AddWithValue(kv.Key, kv.Value ?? DBNull.Value);
                }
            }

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            var result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return ToolEnvelope.Ok(new { value = result == DBNull.Value ? null : result });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"查询失败: {ex.Message}");
        }
    }

    [Description("获取数据库表信息。参数：dataSource（已登记的数据源名）, tableName（可选）。")]
    public async Task<string> GetTableInfoAsync(
        [Description("已登记的数据源名称")] string dataSource,
        [Description("表名（可选），不传则返回所有表")] string? tableName = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.GetTableInfoAsync -> {tableName ?? "All Tables"}");

        if (!TryResolve(dataSource, out var resolved, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            var sql = @"
SELECT 
    t.TABLE_SCHEMA + '.' + t.TABLE_NAME AS TableName,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
    c.IS_NULLABLE,
    c.COLUMN_DEFAULT AS DefaultValue
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME
WHERE t.TABLE_TYPE = 'BASE TABLE'
" + (!string.IsNullOrWhiteSpace(tableName) ? "AND t.TABLE_NAME = @tableName" : "") + @"
ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

            var results = new List<Dictionary<string, object>>();

            using var connection = new SqlConnection(resolved!.ConnectionString);
            using var command = new SqlCommand(sql, connection)
            {
                CommandTimeout = 30
            };

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                command.Parameters.AddWithValue("@tableName", tableName);
            }

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null : value;
                }
                results.Add(row);
            }

            // 按表分组
            var tableGroups = results
                .GroupBy(r => r["TableName"]?.ToString() ?? "")
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new
                {
                    TableName = g.Key,
                    Columns = g.Select(c => new
                    {
                        Name = c["COLUMN_NAME"],
                        DataType = c["DATA_TYPE"],
                        MaxLength = c["MaxLength"],
                        IsNullable = c["IS_NULLABLE"],
                        DefaultValue = c["DefaultValue"]
                    }).ToList()
                })
                .ToList();

            return ToolEnvelope.Ok(new
            {
                tableCount = tableGroups.Count,
                tables = tableGroups
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"获取表信息失败: {ex.Message}");
        }
    }

    [Description("测试数据库连接。参数：dataSource（已登记的数据源名）。")]
    public async Task<string> TestConnectionAsync(
        [Description("已登记的数据源名称")] string dataSource,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🔧 DbTool.TestConnectionAsync");

        if (!TryResolve(dataSource, out var resolved, out var resolveError))
        {
            return ToolEnvelope.Fail(resolveError!);
        }

        try
        {
            using var connection = new SqlConnection(resolved!.ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = new SqlCommand("SELECT @@VERSION", connection);
            var version = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            return ToolEnvelope.Ok(new
            {
                connected = true,
                serverVersion = connection.ServerVersion,
                database = connection.Database,
                sqlServerVersion = version?.ToString()
            });
        }
        catch (Exception ex)
        {
            return ToolEnvelope.Fail($"连接失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 解析 JSON 格式的参数
    /// </summary>
    private static Dictionary<string, object?>? ParseParameters(string? parametersJson)
    {
        if (string.IsNullOrWhiteSpace(parametersJson))
            return null;

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, object?>>(parametersJson);
            return dict;
        }
        catch
        {
            return null;
        }
    }
}
