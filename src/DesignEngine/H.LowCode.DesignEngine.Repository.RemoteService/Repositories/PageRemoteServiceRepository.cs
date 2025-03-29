using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Model;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Repository.RemoteService;

public class PageRemoteServiceRepository : RemoteServiceRepositoryBase, IPageRepository
{
    public PageRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public Task<List<PageListModel>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task<PagePartsSchema> GetByIdAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(PagePartsSchema pageSchema)
    {
        throw new NotImplementedException();
    }

    public Task<PagePartsSchema> GetAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string appId, string pageId)
    {
        throw new NotImplementedException();
    }
}
