using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public class AppDomainService : DomainService, IAppDomainService
{
    private readonly IAppRepository _repository;

    public AppDomainService(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<AppPartsSchema>> GetListAsync()
    {
        return await _repository.GetListAsync();
    }

    public async Task<AppPartsSchema> GetAsync(string appId)
    {
        return await _repository.GetAsync(appId);
    }

    public async Task SaveAsync(AppPartsSchema appSchema)
    {
        await _repository.SaveAsync(appSchema);
    }
}