using H.Abp.Application.Contracts;
using H.LowCode.Application.Contracts;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.RenderEngine.Domain;

public interface ITableDataRepository : IRepository
{
    Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput input);
    Task DeleteAsync(TableDataDeleteInput request);
    Task UpdateAsync(TableDataUpdateInput request);

    /// <summary>
    /// 保存行数据（按主键新增或更新），返回主键值
    /// </summary>
    Task<string> SaveAsync(TableDataSaveInput request);
}
