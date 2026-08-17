using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.DesignEngine.Model;
using H.LowCode.MetaSchema.DesignEngine;
using H.Util.Base;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class ComponentPartsAppService : ApplicationService, IComponentPartsAppService
{
    private IComponentPartsRepository _repository => LazyServiceProvider.GetRequiredService<IComponentPartsRepository>();

    public async Task<BaseOutput<List<ComponentPartsSchema>>> GetAllComponentsAsync(string libraryId)
    {
        return new(await _repository.GetAllComponentsAsync(libraryId));
    }

    public async Task<BaseOutput<ComponentPartsSchema>> GetByIdAsync(string libraryId, string partsId)
    {
        return new(await _repository.GetByIdAsync(libraryId, partsId));
    }

    public async Task<BaseOutput<List<ComponentPartsListModel>>> GetListAsync(string libraryId)
    {
        return new(await _repository.GetListAsync(libraryId));
    }

    public async Task<BaseOutput<bool>> SaveAsync(ComponentPartsSchema componentParts)
    {
        return new(await _repository.SaveAsync(componentParts));
    }

    public async Task<BaseOutput<bool>> DeleteAsync(string libraryId, string partsId)
    {
        return new(await _repository.DeleteAsync(libraryId, partsId));
    }
}
