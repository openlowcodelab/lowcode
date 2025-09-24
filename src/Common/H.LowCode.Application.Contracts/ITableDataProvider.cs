using Volo.Abp;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 表格数据提供者接口
/// </summary>
public interface ITableDataProvider : IRemoteService
{
    /// <summary>
    /// 获取表格数据
    /// </summary>
    /// <param name="request">数据请求参数</param>
    /// <returns>表格数据响应</returns>
    Task<TableDataResponse> GetTableDataAsync(TableDataRequest request);
}

/// <summary>
/// 表格数据请求参数
/// </summary>
public class TableDataRequest
{
    /// <summary>
    /// 应用ID
    /// </summary>
    public string? AppId { get; set; }

    /// <summary>
    /// 页面ID
    /// </summary>
    public string? PageId { get; set; }

    /// <summary>
    /// 数据源ID
    /// </summary>
    public string? DataSourceId { get; set; }

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
    public string? SortField { get; set; }

    /// <summary>
    /// 排序方向
    /// </summary>
    public string? SortOrder { get; set; }

    /// <summary>
    /// 筛选条件
    /// </summary>
    public Dictionary<string, object>? Filters { get; set; }
}

/// <summary>
/// 表格数据响应
/// </summary>
public class TableDataResponse
{
    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<Dictionary<string, object>> Items { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int PageIndex { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
}