using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.RenderEngine.Application.Contracts;

public class TableGetListInput
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
    /// 页码（从1开始）
    /// </summary>
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortField { get; set; }

    /// <summary>
    /// 排序方向（asc/desc）
    /// </summary>
    public string SortOrder { get; set; }

    /// <summary>
    /// 筛选条件
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new Dictionary<string, object>();
}
