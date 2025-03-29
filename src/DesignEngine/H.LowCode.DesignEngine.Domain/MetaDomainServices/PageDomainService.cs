﻿using H.LowCode.DesignEngine.Model;
using H.LowCode.DesignEngine.Domain.Repositories;
using H.LowCode.MetaSchema.DesignEngine;
using Volo.Abp.Domain.Services;

namespace H.LowCode.DesignEngine.Domain;

public class PageDomainService : DomainService, IPageDomainService
{
    private readonly IPageRepository _repository;

    public PageDomainService(IPageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PageListModel>> GetListAsync(string appId)
    {
        return await _repository.GetListAsync(appId);
    }

    public async Task<PagePartsSchema> GetByIdAsync(string appId, string pageId)
    {
        return await _repository.GetByIdAsync(appId, pageId);
    }

    public async Task SaveAsync(PagePartsSchema pageSchema)
    {
        await _repository.SaveAsync(pageSchema);
    }

    public async Task DeleteAsync(string appId, string pageId)
    {
        await _repository.DeleteAsync(appId, pageId);
    }
}