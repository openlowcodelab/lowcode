#nullable enable
using H.LowCode.Application.Contracts;
using H.LowCode.MetaSchema;

namespace H.LowCode.ComponentBase.Services;

/// <summary>
/// 默认表格数据提供者（提供模拟数据）
/// </summary>
public class DefaultTableDataProvider : ITableDataProvider
{
    public async Task<TableDataResponse> GetTableDataAsync(TableDataRequest request)
    {
        await Task.Delay(100); // 模拟异步操作

        // 生成模拟数据
        var items = new List<Dictionary<string, object>>();
        
        // 根据页码和页大小生成数据
        var startIndex = (request.PageIndex - 1) * request.PageSize;
        for (int i = 0; i < 6; i++)
        {
            var rowIndex = startIndex + i + 1;
            var row = new Dictionary<string, object>
            {
                ["Id"] = rowIndex,
                ["Name"] = $"示例数据 {rowIndex}",
                ["CreateTime"] = DateTime.Now.AddDays(-rowIndex)
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