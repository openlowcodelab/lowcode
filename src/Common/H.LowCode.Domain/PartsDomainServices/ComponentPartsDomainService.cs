using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain.Repositories;
using H.LowCode.PartsMetaSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace H.LowCode.Domain;

public class ComponentPartsDomainService : DomainService, IComponentPartsDomainService
{
    private readonly IComponentPartsRepository _repository;

    public ComponentPartsDomainService(IComponentPartsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ComponentPartsListModel>> GetListAsync(string libraryId)
    {
        return await _repository.GetListAsync(libraryId);
    }

    public async Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId)
    {
        return await _repository.GetAllComponentsAsync(libraryId);
    }

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId)
    {
        return await _repository.GetByIdAsync(libraryId, componentId);
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        return await _repository.SaveAsync(componentParts);
    }

    public async Task<bool> DeleteAsync(string libraryId, string componentId)
    {
        return await _repository.DeleteAsync(libraryId, componentId);
    }
}
