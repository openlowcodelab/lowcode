using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.Domain.Repositories;

public interface IAppRepository : IRepository
{
    Task<IList<AppSchema>> GetListAsync();

    Task<AppSchema> GetAsync(string appId);

    Task SaveAsync(AppSchema appSchema);
}