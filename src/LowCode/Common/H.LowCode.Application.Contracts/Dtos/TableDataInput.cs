using System;
using H.Abp.Application.Contracts;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表格数据请求参数
/// </summary>
public class TableDataInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public required string AppId { get; set; }

    /// <summary>
    /// 页面ID
    /// </summary>
    public required string PageId { get; set; }

    /// <summary>
    /// 数据源ID
    /// </summary>
    public required string? DataSourceId { get; set; }

    /// <summary>
    /// 筛选条件
    /// </summary>
    public Dictionary<string, object>? Filters { get; set; }
}