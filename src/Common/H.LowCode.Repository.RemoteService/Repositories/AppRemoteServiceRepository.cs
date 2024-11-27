using H.LowCode.Configuration;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.Repository.RemoteService;

public class AppRemoteServiceRepository : RemoteServiceRepositoryBase, IAppRepository
{
    public AppRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {
        
    }

    public bool? IsChangeTrackingEnabled => false;

    public async Task<AppSchema> GetAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public async Task<IList<AppSchema>> GetListAsync()
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(AppSchema appSchema)
    {
        throw new NotImplementedException();
    }
}
