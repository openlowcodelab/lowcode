namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表格数据保存请求（按主键新增或更新）
/// </summary>
public class TableDataSaveInput
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 数据源ID
    /// </summary>
    public string DataSourceId { get; set; }

    /// <summary>
    /// 行数据（字段名 -> 值）
    /// </summary>
    /// <remarks>主键为空时自动生成并新增，主键存在时更新（不存在则新增）</remarks>
    public Dictionary<string, object> RowData { get; set; }

    /// <summary>
    /// 强制新增（忽略行数据中的主键，重新生成主键后插入）
    /// </summary>
    public bool ForceInsert { get; set; }
}
