using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.Repository.RemoteService;

public class DataSourceRemoteServiceRepository : RemoteServiceRepositoryBase, IDataSourceRepository
{
    public DataSourceRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public async Task DeleteAsync(string appId, string id)
    {
        throw new NotImplementedException();
    }

    public async Task<DataSourceSchema> GetAsync(string appId, string id)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<DataSourceSchema>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(string appId, DataSourceSchema dataSourceSchema)
    {
        throw new NotImplementedException();
    }
}
