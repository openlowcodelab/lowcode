using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageAppService : ApplicationService, IPageAppService
{
    private IPageDomainService _domainService => LazyServiceProvider.GetRequiredService<IPageDomainService>();

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        return await _domainService.GetListAsync(appId);
    }

    public async Task<PagePartsSchema> GetByIdAsync(string appId, string pageId)
    {
        return await _domainService.GetByIdAsync(appId, pageId);
    }

    public async Task<bool> SaveAsync(PagePartsSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        await _domainService.SaveAsync(pageSchema);
        return true;
    }

    public async Task<bool> DeleteAsync(string appId, string pageId)
    {
        await _domainService.DeleteAsync(appId, pageId);
        return true;
    }
}
