using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppApplicationService : IApplicationService
{
    Task<IList<AppListModel>> GetAppsAsync();

    Task<IList<AppPartsSchema>> GetListAsync();

    Task<AppPartsSchema> GetByIdAsync(string appId);

    Task<bool> SaveAsync(AppPartsSchema appSchema);
}