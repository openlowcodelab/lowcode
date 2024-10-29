using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public interface IAppDomainService : IDomainService
{
    Task<IList<AppSchema>> GetListAsync();

    Task SaveAsync(AppSchema appSchema);

    Task<AppSchema> GetAsync(string appId);
}