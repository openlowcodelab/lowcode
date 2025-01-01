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

public class ComponentLibraryDomainService : DomainService, IComponentLibraryDomainService
{
    private readonly IComponentLibraryRepository _repository;

    public ComponentLibraryDomainService(IComponentLibraryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ComponentLibrarySchema>> GetListAsync()
    {
        return await _repository.GetListAsync();
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
