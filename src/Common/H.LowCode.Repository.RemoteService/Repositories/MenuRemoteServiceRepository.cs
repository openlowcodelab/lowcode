using H.LowCode.Configuration;
using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.Repository.RemoteService;

public class MenuRemoteServiceRepository : RemoteServiceRepositoryBase, IMenuRepository, IScopedDependency
{
    public MenuRemoteServiceRepository(IOptions<MetaOption> metaOption)
    {

    }

    public Task DeleteAsync(string appId, string menuId)
    {
        throw new NotImplementedException();
    }

    public Task<MenuSchema> GetAsync(string appId, string menuId)
    {
        throw new NotImplementedException();
    }

    public Task<IList<MenuSchema>> GetListAsync(string appId)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(MenuSchema menuSchema)
    {
        throw new NotImplementedException();
    }
}
