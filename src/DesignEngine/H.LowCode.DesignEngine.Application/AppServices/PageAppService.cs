using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class PageAppService : IPageAppService, IScopedDependency
{
    private IPageDomainService _domainService;

    public PageAppService(IPageDomainService domainService)
    {
        _domainService = domainService;
    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        return await _domainService.GetListAsync(appId);
    }

    public async Task<PageSchema> GetByIdAsync(string appId, string pageId)
    {
        return await _domainService.GetAsync(appId, pageId);
    }

    public async Task SaveAsync(PageSchema pageSchema)
    {
        ArgumentNullException.ThrowIfNull(pageSchema);
        ArgumentException.ThrowIfNullOrEmpty(pageSchema.Id);

        await _domainService.SaveAsync(pageSchema);
    }

    public async Task DeleteAsync(string appId, string pageId)
    {
        await _domainService.DeleteAsync(appId, pageId);
    }
}
