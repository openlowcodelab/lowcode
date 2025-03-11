using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.RenderEngine.Domain;

public interface IDataSourceDomainService : IDomainService
{
    Task<IList<DataSourceSchema>> GetListAsync(string appId);

    Task<IList<DataSourceSchema>> GetAllApisAsync(string appId);

    IEnumerable<DataSourceSchema> GetAllEntities(string appId);

    Task<DataSourceSchema> GetAsync(string appId, string id);
}