using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Repository.RemoteService;

public class PageRemoteServiceRepository : RemoteServiceRepositoryBase, IPageRepository, IScopedDependency
{
    public PageRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public Task DeleteAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public Task<PageSchema> GetAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public Task<List<PageListModel>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(PageSchema pageSchema)
    {
        throw new NotImplementedException();
    }
}
