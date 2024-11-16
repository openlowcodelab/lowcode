using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDataSourceAppService : IApplicationService
{
    Task<IList<DataSourceListModel>> GetListAsync(string appId, DataSourceInput input);

    Task<DataSourceSchema> GetByIdAsync(string appId, string id);

    Task<bool> SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task<bool> DeleteAsync(string appId, string id);
}