using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Domain.Repositories;

public interface IAppRepository
{
    Task<IList<AppSchema>> GetListAsync();

    Task<AppSchema> GetAsync(string appId);
}