using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class ComponentPartsAppService : ApplicationService, IComponentPartsAppService
{
    private IComponentPartsRepository _repository => LazyServiceProvider.GetRequiredService<IComponentPartsRepository>();

    public async Task<bool> DeleteAsync(string libraryId, string componentId)
    {
        return await _repository.DeleteAsync(libraryId, componentId);
    }

    public async Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId)
    {
        return await _repository.GetAllComponentsAsync(libraryId);
    }

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId)
    {
        return await _repository.GetByIdAsync(libraryId, componentId);
    }

    public async Task<List<ComponentPartsListModel>> GetListAsync(string libraryId)
    {
        return await _repository.GetListAsync(libraryId);
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        return await _repository.SaveAsync(componentParts);
    }
}
