using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppApplicationService : IApplicationService
{
    Task<IList<AppSchema>> GetListAsync();

    Task<AppSchema> GetAsync(string appId);

    Task SaveAsync(AppSchema appSchema);
}