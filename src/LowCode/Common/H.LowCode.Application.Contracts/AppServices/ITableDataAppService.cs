using H.Abp.Application.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace H.LowCode.Application.Contracts;

/// <summary>
/// 列表数据查询
/// </summary>
public interface ITableDataAppService : IAppService
{
    /// <summary>
    /// 获取表格数据
    /// </summary>
    Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput request);

    /// <summary>
    /// 删除数据
    /// </summary>
    Task DeleteAsync(TableDataDeleteInput request);

    /// <summary>
    /// 更新数据
    /// </summary>
    Task UpdateAsync(TableDataUpdateInput request);
}
