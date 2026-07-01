using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class ComponentLibraryAppService : ApplicationService, IComponentLibraryAppService
{
    private IComponentLibraryRepository _repository => LazyServiceProvider.GetRequiredService<IComponentLibraryRepository>();

    public Task<List<ComponentLibrarySchema>> GetListAsync()
    {
        return _repository.GetListAsync();
    }

    public async Task<ComponentLibrarySchema> GetByIdAsync(string libraryId)
    {
        return await _repository.GetByIdAsync(libraryId);
    }

    public async Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary)
    {
        return await _repository.SaveAsync(componentLibrary);
    }

    public async Task<bool> DeleteAsync(string libraryId)
    {
        return await _repository.DeleteAsync(libraryId);
    }
}
