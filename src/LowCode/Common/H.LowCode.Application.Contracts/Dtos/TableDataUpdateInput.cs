using System;
using System.Collections.Generic;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表格数据更新请求参数
/// </summary>
public class TableDataUpdateInput
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
    /// 主键值
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 行数据
    /// </summary>
    public Dictionary<string, object> RowData { get; set; }

    /// <summary>
    /// 更新的字段数据
    /// </summary>
    public Dictionary<string, object> UpdateData { get; set; }
}