using H.LowCode.Application.Contracts;
using H.LowCode.RenderEngine.Application.Contracts;
using H.LowCode.RenderEngine.Domain;

namespace H.LowCode.RenderEngine.Application.DataAppServices;

/// <summary>
/// 渲染引擎表格数据提供者
/// </summary>
public class RenderEngineTableDataProvider : ITableDataProvider
{
    private readonly ITableDataDomainService _tableDataDomainService;

    public RenderEngineTableDataProvider(ITableDataDomainService tableDataDomainService)
    {
        _tableDataDomainService = tableDataDomainService;
    }

    public async Task<TableDataResponse> GetTableDataAsync(TableDataRequest request)
    {
        try
        {
            // 检查是否配置了数据源ID
            if (string.IsNullOrEmpty(request.DataSourceId))
            {
                // 返回默认模拟数据
                return await GetDefaultDataAsync(request);
            }

            // 转换为Domain层的输入参数
            var input = new TableGetListInput
            {
                AppId = request.AppId,
                PageId = request.PageId,
                DataSourceId = request.DataSourceId,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                SortField = request.SortField,
                SortOrder = request.SortOrder,
                Filters = request.Filters ?? new Dictionary<string, object>()
            };

            // 调用Domain层服务获取数据
            var result = await _tableDataDomainService.GetListAsync(input);

            // 转换为组件层的响应格式
            return new TableDataResponse
            {
                Items = result.Items,
                TotalCount = result.TotalCount,
                PageIndex = result.PageIndex,
                PageSize = result.PageSize
            };
        }
        catch (Exception ex)
        {
            // 出错时返回默认数据
            Console.WriteLine($"获取表格数据失败: {ex.Message}");
            return await GetDefaultDataAsync(request);
        }
    }

    private async Task<TableDataResponse> GetDefaultDataAsync(TableDataRequest request)
    {
        await Task.Delay(100); // 模拟异步操作

        // 生成模拟数据
        var items = new List<Dictionary<string, object>>();
        
        // 根据页码和页大小生成数据
        var startIndex = (request.PageIndex - 1) * request.PageSize;
        for (int i = 0; i < request.PageSize; i++)
        {
            var rowIndex = startIndex + i + 1;
            var row = new Dictionary<string, object>
            {
                ["Id"] = rowIndex,
                ["Name"] = $"示例数据 {rowIndex}",
                ["Status"] = rowIndex % 2 == 0 ? "启用" : "禁用",
                ["CreateTime"] = DateTime.Now.AddDays(-rowIndex),
                ["Description"] = $"这是第 {rowIndex} 行的描述信息"
            };
            items.Add(row);
        }

        return new TableDataResponse
        {
            Items = items,
            TotalCount = 100, // 模拟总数
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}