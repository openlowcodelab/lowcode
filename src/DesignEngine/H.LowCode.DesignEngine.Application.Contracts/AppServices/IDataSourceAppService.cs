using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Application.Contracts;

public interface IDataSourceAppService
{
    Task<IList<DataSourceListModel>> GetListAsync(string appId, DataSourceInput input);

    Task<DataSourceSchema> GetAsync(string appId, string id);

    Task SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task DeleteAsync(string appId, string id);
}