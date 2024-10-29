using H.LowCode.Domain.Repositories;
using H.LowCode.MetaSchema;

namespace H.LowCode.Domain;

public class AppDomainService : IAppDomainService
{
    private readonly IAppRepository _repository;

    public AppDomainService(IAppRepository repository,
        IFormDataRepository formDataRepository
        )
    {
        _repository = repository;

        formDataRepository.Test();  //TODO: 测试
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