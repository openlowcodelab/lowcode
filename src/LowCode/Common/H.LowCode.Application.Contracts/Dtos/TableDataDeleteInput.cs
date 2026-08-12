namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表格数据删除请求参数
/// </summary>
public class TableDataDeleteInput
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// 页面ID
    /// </summary>
    public string PageId { get; set; }

    /// <summary>
    /// 数据源ID
    /// </summary>
    public string DataSourceId { get; set; }

    /// <summary>
    /// 要删除的记录ID
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 要删除的记录数据（包含主键等信息）
    /// </summary>
    public Dictionary<string, object> RowData { get; set; }
}