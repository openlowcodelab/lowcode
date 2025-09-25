using H.LowCode.Application.Contracts;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.RenderEngine.Domain;

public interface ITableDataRepository : IRepository
{
    Task<PagedResultDto<Dictionary<string, object>>> GetListAsync(TableDataInput input);
}
