using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public interface IAppDomainService : IDomainService
{
    Task<IList<AppPartsSchema>> GetListAsync();

    Task SaveAsync(AppPartsSchema appSchema);

    Task<AppPartsSchema> GetAsync(string appId);
}