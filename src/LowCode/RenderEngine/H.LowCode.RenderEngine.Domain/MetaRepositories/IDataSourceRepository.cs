using H.LowCode.MetaSchema;

namespace H.LowCode.RenderEngine.Domain.Repositories;

public interface IDataSourceRepository
{
    Task<IList<DataSourceSchema>> GetListAsync(string appId);

    Task<DataSourceSchema> GetAsync(string appId, string id);

    Task<IList<DataSourceSchema>> GetAllApisAsync(string appId);

    IEnumerable<DataSourceSchema> GetAllEntities(string appId);
}