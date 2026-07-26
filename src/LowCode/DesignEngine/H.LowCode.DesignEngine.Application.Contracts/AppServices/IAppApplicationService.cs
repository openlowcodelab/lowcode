using H.Abstractions;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppApplicationService : IAppService
{
    Task<IList<AppListModel>> GetAppsAsync();

    Task<IList<AppPartsSchema>> GetListAsync();

    Task<AppPartsSchema> GetByIdAsync(string appId);

    Task<bool> SaveAsync(AppPartsSchema appSchema);

    Task<bool> DeleteAsync(string appId);
}