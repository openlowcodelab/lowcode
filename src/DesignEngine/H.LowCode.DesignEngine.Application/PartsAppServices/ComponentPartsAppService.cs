using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Model;
using H.LowCode.Domain;
using H.LowCode.PartsMetaSchema;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace H.LowCode.DesignEngine.Application;

[RemoteService]
public class ComponentPartsAppService : ApplicationService, IComponentPartsAppService
{
    private IComponentPartsDomainService _domainService => LazyServiceProvider.GetRequiredService<IComponentPartsDomainService>();

    public async Task<bool> DeleteAsync(string libraryId, string componentId)
    {
        return await _domainService.DeleteAsync(libraryId, componentId);
    }

    public async Task<List<ComponentPartsSchema>> GetAllComponentsAsync(string libraryId)
    {
        return await _domainService.GetAllComponentsAsync(libraryId);
    }

    public async Task<ComponentPartsSchema> GetByIdAsync(string libraryId, string componentId)
    {
        return await _domainService.GetByIdAsync(libraryId, componentId);
    }

    public async Task<List<ComponentPartsListModel>> GetListAsync(string libraryId)
    {
        return await _domainService.GetListAsync(libraryId);
    }

    public async Task<bool> SaveAsync(ComponentPartsSchema componentParts)
    {
        return await _domainService.SaveAsync(componentParts);
    }
}
