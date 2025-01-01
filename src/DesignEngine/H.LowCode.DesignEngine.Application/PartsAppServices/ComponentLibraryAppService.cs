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
public class ComponentLibraryAppService : ApplicationService, IComponentLibraryAppService
{
    private IComponentLibraryDomainService _domainService => LazyServiceProvider.GetRequiredService<IComponentLibraryDomainService>();

    public Task<List<ComponentLibrarySchema>> GetListAsync()
    {
        return _domainService.GetListAsync();
    }

    public async Task<ComponentLibrarySchema> GetByIdAsync(string libraryId)
    {
        return await _domainService.GetByIdAsync(libraryId);
    }

    public async Task<bool> SaveAsync(ComponentLibrarySchema componentLibrary)
    {
        return await _domainService.SaveAsync(componentLibrary);
    }

    public async Task<bool> DeleteAsync(string libraryId)
    {
        return await _domainService.DeleteAsync(libraryId);
    }
}
