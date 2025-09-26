using H.LowCode.Application.Contracts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.LowCode.ComponentBase.Services;

/// <summary>
/// 默认表格数据提供者（提供模拟数据）
/// </summary>
public class TableDataAppService : ApplicationService, ITableDataAppService
{
    public async Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput request)
    {
        await Task.Delay(100); // 模拟异步操作

        // 生成模拟数据
        var items = new List<Dictionary<string, object>>();

        // 根据页码和页大小生成数据
        for (int i = 0; i < 3; i++)
        {
            var rowIndex = i + 1;
            var row = new Dictionary<string, object>
            {
                ["Id"] = rowIndex,
                ["CreateTime"] = DateTime.Now.AddDays(-rowIndex)
            };
            items.Add(row);
        }

        return new()
        {
            Items = items,
            TotalCount = 3
        };
    }

    public async Task DeleteAsync(TableDataDeleteInput request)
    {
        await Task.Delay(100); // 模拟异步操作
        
        // 在设计引擎中，这只是模拟删除操作
        // 实际上不会删除任何数据，只是为了演示功能
        Console.WriteLine($"模拟删除数据: AppId={request.AppId}, PageId={request.PageId}, DataSourceId={request.DataSourceId}, Id={request.Id}");
    }

    public async Task UpdateAsync(TableDataUpdateInput request)
    {
        await Task.Delay(100); // 模拟异步操作
        
        // 在设计引擎中，这只是模拟更新操作
        // 实际上不会更新任何数据，只是为了演示功能
        Console.WriteLine($"模拟更新数据: AppId={request.AppId}, PageId={request.PageId}, DataSourceId={request.DataSourceId}, Id={request.Id}");
        Console.WriteLine($"更新字段: {string.Join(", ", request.UpdateData?.Select(kv => $"{kv.Key}={kv.Value}") ?? new string[0])}");
    }
}