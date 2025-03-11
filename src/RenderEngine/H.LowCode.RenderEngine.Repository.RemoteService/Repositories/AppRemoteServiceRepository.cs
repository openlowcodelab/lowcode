using H.LowCode.Configuration;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Domain.Repositories;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.RenderEngine.Repository.RemoteService;

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
}
