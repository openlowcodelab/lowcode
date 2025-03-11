using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IAppRepository
{
    Task<IList<AppPartsSchema>> GetListAsync();

    Task<AppPartsSchema> GetAsync(string appId);

    Task SaveAsync(AppPartsSchema appSchema);
}