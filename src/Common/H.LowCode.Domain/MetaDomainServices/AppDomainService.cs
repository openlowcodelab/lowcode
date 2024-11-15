using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public class AppDomainService : DomainService, IAppDomainService
{
    private readonly IAppRepository _repository;

    public AppDomainService(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<IList<AppSchema>> GetListAsync()
    {
        return await _repository.GetListAsync();
    }

    public async Task<AppSchema> GetAsync(string appId)
    {
        return await _repository.GetAsync(appId);
    }

    public async Task SaveAsync(AppSchema appSchema)
    {
        await _repository.SaveAsync(appSchema);
    }
}