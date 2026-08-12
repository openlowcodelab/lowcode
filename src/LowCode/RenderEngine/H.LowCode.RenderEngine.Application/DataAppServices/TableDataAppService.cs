using H.Abp.Application.Contracts;
using H.LowCode.Application.Contracts;
using H.LowCode.RenderEngine.Domain;
using Volo.Abp.Application.Services;

namespace H.LowCode.RenderEngine.Application;

/// <summary>
/// 渲染引擎表格数据提供者
/// </summary>
public class TableDataAppService : ApplicationService, ITableDataAppService
{
    private readonly ITableDataRepository _tableDataRepository;

    public TableDataAppService(ITableDataRepository tableDataRepository)
    {
        _tableDataRepository = tableDataRepository;
    }

    public async Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput input)
    {
        return await _tableDataRepository.GetListAsync(input);
    }

    public async Task DeleteAsync(TableDataDeleteInput request)
    {
        await _tableDataRepository.DeleteAsync(request);
    }

    public async Task UpdateAsync(TableDataUpdateInput request)
    {
        await _tableDataRepository.UpdateAsync(request);
    }
}