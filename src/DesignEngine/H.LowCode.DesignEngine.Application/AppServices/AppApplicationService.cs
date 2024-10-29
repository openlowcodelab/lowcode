using H.Extensions.System;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace H.LowCode.DesignEngine.Application;

public class AppApplicationService : IAppApplicationService
{
    private IAppDomainService _domainService;

    public AppApplicationService(IAppDomainService domainService)
    {
        _domainService = domainService;
    }

    public async Task<IList<AppSchema>> GetListAsync()
    {
        return await _domainService.GetListAsync();
    }

    public async Task<AppSchema> GetAsync(string appId)
    {
        return await _domainService.GetAsync(appId);
    }

    public async Task SaveAsync(AppSchema appSchema)
    {
        ArgumentNullException.ThrowIfNull(appSchema);
        ArgumentException.ThrowIfNullOrEmpty(appSchema.Id);

        await _domainService.SaveAsync(appSchema);
    }
}
