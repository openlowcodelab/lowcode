using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public interface IDataSourceDomainService : IDomainService
{
    Task<IList<DataSourceSchema>> GetListAsync(string appId);

    Task<IList<DataSourceSchema>> GetAllApisAsync(string appId);

    IEnumerable<DataSourceSchema> GetAllEntities(string appId);

    Task<DataSourceSchema> GetAsync(string appId, string id);

    Task SaveAsync(string appId, DataSourceSchema dataSourceSchema);

    Task DeleteAsync(string appId, string id);
}