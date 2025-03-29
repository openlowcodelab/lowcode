using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.RemoteService;

public class AppRemoteServiceRepository : RemoteServiceRepositoryBase, IAppRepository
{
    public AppRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {
        
    }

    public bool? IsChangeTrackingEnabled => false;

    public Task<AppPartsSchema> GetAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task<IList<AppPartsSchema>> GetListAsync()
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(AppPartsSchema appSchema)
    {
        throw new NotImplementedException();
    }
}
