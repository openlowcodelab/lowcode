using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppApplicationService : ApplicationService, IAppApplicationService
{
    private IEnumerable<SiteOption> _sites => LazyServiceProvider.GetRequiredService<IOptions<List<SiteOption>>>().Value;

    private IAppDomainService _domainService => LazyServiceProvider.GetRequiredService<AppDomainService>();

    public async Task<IList<AppListModel>> GetAppsAsync()
    {
        var appSchemas = await GetListAsync();
        return appSchemas.Select(x => new AppListModel
        {
            Id = x.Id,
            SiteUrl = _sites.FirstOrDefault(t => t.AppId.Equals(x.Id, StringComparison.OrdinalIgnoreCase))?.SiteUrl,
            Name = x.Name,
            Description = x.Description
        }).ToList();
    }

    public async Task<IList<AppSchema>> GetListAsync()
    {
        return await _domainService.GetListAsync();
    }

    public async Task<AppSchema> GetByIdAsync(string appId)
    {
        return await _domainService.GetAsync(appId);
    }

    public async Task<bool> SaveAsync(AppSchema appSchema)
    {
        ArgumentNullException.ThrowIfNull(appSchema);
        ArgumentException.ThrowIfNullOrEmpty(appSchema.Id);

        await _domainService.SaveAsync(appSchema);
        return true;
    }
}
