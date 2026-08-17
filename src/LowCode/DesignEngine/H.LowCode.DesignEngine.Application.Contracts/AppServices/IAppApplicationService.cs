using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IAppApplicationService : IAppService
{
    Task<BaseOutput<IList<AppListModel>>> GetAppsAsync();

    Task<BaseOutput<IList<AppPartsSchema>>> GetListAsync();

    Task<BaseOutput<AppPartsSchema>> GetByIdAsync(string appId);

    Task<BaseOutput<bool>> SaveAsync(AppPartsSchema appSchema);

    Task<BaseOutput<bool>> DeleteAsync(string appId);
}