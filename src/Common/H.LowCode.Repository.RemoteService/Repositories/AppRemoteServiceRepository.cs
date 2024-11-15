using H.LowCode.Configuration;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Repository.RemoteService;

public class AppRemoteServiceRepository : RemoteServiceRepositoryBase, IAppRepository, IScopedDependency
{
    public AppRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {
        
    }

    public bool? IsChangeTrackingEnabled => false;

    public Task<AppSchema> GetAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task<IList<AppSchema>> GetListAsync()
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(AppSchema appSchema)
    {
        throw new NotImplementedException();
    }
}
