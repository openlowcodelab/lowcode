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
        for (int i = 0; i < request.MaxResultCount; i++)
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
            TotalCount = request.MaxResultCount * 2
        };
    }
}