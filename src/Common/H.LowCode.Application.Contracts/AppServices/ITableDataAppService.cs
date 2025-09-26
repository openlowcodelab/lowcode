using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 列表数据查询
/// </summary>
public interface ITableDataAppService : IApplicationService
{
    /// <summary>
    /// 获取表格数据
    /// </summary>
    Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput request);
}
