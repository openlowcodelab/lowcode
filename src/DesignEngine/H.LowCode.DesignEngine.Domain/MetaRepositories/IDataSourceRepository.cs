using H.LowCode.MetaSchema;

namespace H.LowCode.DesignEngine.Domain.Repositories;

public interface IDataSourceRepository
{
    Task<IList<DataSourceSchema>> GetListAsync(string appId);

    IList<DataSourceSchema> GetAllEntities(string appId);

    Task<DataSourceSchema> GetAsync(string appId, string id);

    Task SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task DeleteAsync(string appId, string id);
}