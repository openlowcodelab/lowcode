using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppApplicationService : ApplicationService, IAppApplicationService
{
    private IEnumerable<SiteOption> _sites => LazyServiceProvider.GetRequiredService<IOptions<List<SiteOption>>>().Value;

    private IAppRepository _repository => LazyServiceProvider.GetRequiredService<IAppRepository>();

    public async Task<IList<AppListModel>> GetAppsAsync()
    {
        var appSchemas = await GetListAsync();
        var apps = appSchemas.Select(x => new AppListModel
        {
            Id = x.Id,
            SiteUrl = _sites.FirstOrDefault(t => t.AppId.Equals(x.Id, StringComparison.OrdinalIgnoreCase))?.SiteUrl,
            Name = x.Name,
            Description = x.Description,
            Order = x.Order
        });

        return [.. apps.OrderBy(t => t.Order)];
    }

    public async Task<IList<AppPartsSchema>> GetListAsync()
    {
        return await _repository.GetListAsync();
    }

    public async Task<AppPartsSchema> GetByIdAsync(string appId)
    {
        return await _repository.GetAsync(appId);
    }

    public async Task<bool> SaveAsync(AppPartsSchema appSchema)
    {
        ArgumentNullException.ThrowIfNull(appSchema);
        ArgumentException.ThrowIfNullOrEmpty(appSchema.Id);

        await _repository.SaveAsync(appSchema);
        return true;
    }
}
