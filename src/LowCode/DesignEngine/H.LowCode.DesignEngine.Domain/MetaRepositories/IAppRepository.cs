using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IAppRepository
{
    Task<IList<AppPartsSchema>> GetListAsync();

    Task<AppPartsSchema> GetAsync(string appId);

    Task SaveAsync(AppPartsSchema appSchema);

    Task DeleteAsync(string appId);
}