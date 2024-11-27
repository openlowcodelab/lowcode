using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.Repository.RemoteService;

public class PageRemoteServiceRepository : RemoteServiceRepositoryBase, IPageRepository
{
    public PageRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public async Task DeleteAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public async Task<PageSchema> GetAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync(PageSchema pageSchema)
    {
        throw new NotImplementedException();
    }
}
