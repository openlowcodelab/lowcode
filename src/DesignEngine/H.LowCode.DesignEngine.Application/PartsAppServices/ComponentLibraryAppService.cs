﻿using H.LowCode.DesignEngine.Application.Contracts;
using H.LowCode.DesignEngine.Domain;
using H.LowCode.MetaSchema.DesignEngine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
