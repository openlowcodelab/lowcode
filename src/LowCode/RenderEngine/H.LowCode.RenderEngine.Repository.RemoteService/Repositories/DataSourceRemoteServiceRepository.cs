using H.LowCode.Configuration;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace H.LowCode.RenderEngine.Repository.RemoteService;

public class DataSourceRemoteServiceRepository : RemoteServiceRepositoryBase, IDataSourceRepository
{
    public DataSourceRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        throw new NotImplementedException();
    }

    public Task<IList<DataSourceSchema>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task<IList<DataSourceSchema>> GetAllApisAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<DataSourceSchema> GetAllEntities(string appId)
    {
        throw new NotImplementedException();
    }
}
