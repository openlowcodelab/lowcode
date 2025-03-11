using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public interface IAppDomainService : IDomainService
{
    Task<IList<AppSchema>> GetListAsync();

    Task<AppSchema> GetAsync(string appId);
}