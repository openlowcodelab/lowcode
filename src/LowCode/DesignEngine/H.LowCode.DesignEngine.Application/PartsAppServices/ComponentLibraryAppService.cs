using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class ComponentLibraryAppService : ApplicationService, IComponentLibraryAppService
{
    private IComponentLibraryRepository _repository => LazyServiceProvider.GetRequiredService<IComponentLibraryRepository>();

    public async Task<BaseOutput<List<ComponentLibrarySchema>>> GetListAsync()
    {
        return new(await _repository.GetListAsync());
    }

    public async Task<BaseOutput<ComponentLibrarySchema>> GetByIdAsync(string libraryId)
    {
        return new(await _repository.GetByIdAsync(libraryId));
    }

    public async Task<BaseOutput<bool>> SaveAsync(ComponentLibrarySchema componentLibrary)
    {
        return new(await _repository.SaveAsync(componentLibrary));
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string libraryId)
    {
        return new(await _repository.DeleteAsync(libraryId));
    }
}
