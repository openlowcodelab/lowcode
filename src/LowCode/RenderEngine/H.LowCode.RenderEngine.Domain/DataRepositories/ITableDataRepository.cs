using H.LowCode.Application.Contracts;
using H.Abstractions;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.RenderEngine.Domain;

public interface ITableDataRepository : IRepository
{
    Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput input);
    Task DeleteAsync(TableDataDeleteInput request);
    Task UpdateAsync(TableDataUpdateInput request);
}
