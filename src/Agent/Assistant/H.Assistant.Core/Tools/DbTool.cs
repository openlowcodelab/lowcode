using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Text.Json;

namespace H.Assistant.Core.Tools;

/// <summary>
/// 数据库访问工具 - 提供 SQL 查询和数据操作功能
/// </summary>
public class DbTool
{
    [Description("执行 SQL 查询。参数：connectionString, sql, parameters, commandTimeout, cancellationToken。")]
    public static async Task<string> ExecuteQueryAsync(
        [Description("数据库连接字符串")] string connectionString,
        [Description("SQL 查询语句")] string sql,
        [Description("查询参数（JSON 格式），如 {\"@name\": \"value\"}，可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteQueryAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        try
        {
            var paramDict = ParseParameters(parameters);
            var results = new List<Dictionary<string, object>>();

            using var connection = new SqlConnection(connectionString);
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

            var result = new
            {
                Success = true,
                RowCount = results.Count,
                Data = results
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 查询失败: {ex.Message}";
        }
    }

    [Description("执行 SQL 命令（INSERT/UPDATE/DELETE）。参数：connectionString, sql, parameters, commandTimeout, cancellationToken。")]
    public static async Task<string> ExecuteCommandAsync(
        [Description("数据库连接字符串")] string connectionString,
        [Description("SQL 命令语句")] string sql,
        [Description("命令参数（JSON 格式），可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteCommandAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        try
        {
            var paramDict = ParseParameters(parameters);

            using var connection = new SqlConnection(connectionString);
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

            var result = new
            {
                Success = true,
                AffectedRows = affectedRows
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 执行失败: {ex.Message}";
        }
    }

    [Description("执行标量查询（返回单个值）。参数：connectionString, sql, parameters, commandTimeout, cancellationToken。")]
    public static async Task<string> ExecuteScalarAsync(
        [Description("数据库连接字符串")] string connectionString,
        [Description("SQL 查询语句")] string sql,
        [Description("查询参数（JSON 格式），可为 null")] string? parameters = null,
        [Description("命令超时（秒），默认 30 秒")] int commandTimeout = 30,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.ExecuteScalarAsync -> {sql[..Math.Min(50, sql.Length)]}...");

        try
        {
            var paramDict = ParseParameters(parameters);

            using var connection = new SqlConnection(connectionString);
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

            var response = new
            {
                Success = true,
                Value = result == DBNull.Value ? null : result
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 查询失败: {ex.Message}";
        }
    }

    [Description("获取数据库表信息。参数：connectionString, tableName（可选）, cancellationToken。")]
    public static async Task<string> GetTableInfoAsync(
        [Description("数据库连接字符串")] string connectionString,
        [Description("表名（可选），不传则返回所有表")] string? tableName = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔧 DbTool.GetTableInfoAsync -> {tableName ?? "All Tables"}");

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

            using var connection = new SqlConnection(connectionString);
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

            var response = new
            {
                Success = true,
                TableCount = tableGroups.Count,
                Tables = tableGroups
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return $"❌ 获取表信息失败: {ex.Message}";
        }
    }

    [Description("测试数据库连接。参数：connectionString, cancellationToken。")]
    public static async Task<string> TestConnectionAsync(
        [Description("数据库连接字符串")] string connectionString,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("🔧 DbTool.TestConnectionAsync");

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

            using var command = new SqlCommand("SELECT @@VERSION", connection);
            var version = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);

            var result = new
            {
                Success = true,
                Message = "连接成功",
                ServerVersion = connection.ServerVersion,
                Database = connection.Database,
                SqlServerVersion = version?.ToString()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = false });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "连接失败",
                Error = ex.Message
            }, new JsonSerializerOptions { WriteIndented = false });
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
