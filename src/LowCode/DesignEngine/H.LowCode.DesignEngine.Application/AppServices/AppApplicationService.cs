using H.LowCode.Configuration;
using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class AppApplicationService : ApplicationService, IAppApplicationService
{
    private readonly IEnumerable<SiteOption> _sites;
    private readonly IAppRepository _repository;

    public AppApplicationService(IOptions<List<SiteOption>> siteOptions, IAppRepository repository)
    {
        _sites = siteOptions.Value;
        _repository = repository;
    }

    public async Task<BaseOutput<IList<AppListModel>>> GetAppsAsync()
    {
        var appSchemas = (await GetListAsync()).Data ?? [];
        var apps = appSchemas.Select(x => new AppListModel
        {
            Id = x.Id,
            SiteUrl = _sites.FirstOrDefault(t => t.AppId.Equals(x.Id, StringComparison.OrdinalIgnoreCase))?.SiteUrl,
            Name = x.Name,
            Description = x.Description,
            Order = x.Order,
            PublishStatus = x.PublishStatus
        });

        return new([.. apps.OrderBy(t => t.Order)]);
    }

    public async Task<BaseOutput<IList<AppPartsSchema>>> GetListAsync()
    {
        return new(await _repository.GetListAsync());
    }

    public async Task<BaseOutput<AppPartsSchema>> GetByIdAsync(string appId)
    {
        return new(await _repository.GetAsync(appId));
    }

    public async Task<BaseOutput<bool>> SaveAsync(AppPartsSchema appSchema)
    {
        ArgumentNullException.ThrowIfNull(appSchema);
        ArgumentException.ThrowIfNullOrEmpty(appSchema.Id);

        await _repository.SaveAsync(appSchema);
        return new(true);
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string appId)
    {
        ArgumentException.ThrowIfNullOrEmpty(appId);

        await _repository.DeleteAsync(appId);
        return new(true);
    }
}
