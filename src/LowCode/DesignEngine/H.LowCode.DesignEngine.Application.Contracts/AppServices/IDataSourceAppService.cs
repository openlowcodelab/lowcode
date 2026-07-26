using H.Abp.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDataSourceAppService : IAppService
{
    Task<IList<DataSourceListModel>> GetListAsync(string appId, DataSourceInput input);

    Task<DataSourceSchema> GetByIdAsync(string appId, string id);

    Task<bool> SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task<bool> DeleteAsync(string appId, string id);
}