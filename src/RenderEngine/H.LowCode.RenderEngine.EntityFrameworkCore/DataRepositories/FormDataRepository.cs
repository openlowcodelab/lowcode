﻿using H.LowCode.Entity;
using H.LowCode.MetaSchema;
using H.LowCode.RenderEngine.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    private RenderEngineDbContext _dbContext;
    public bool? IsChangeTrackingEnabled => true;

    public FormDataRepository(RenderEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddAsync(FormEntity entity)
    {
        return await _dbContext.AddAsync(entity);
    }

    public async Task<FormEntity> GetAsync(string tableName, string id)
    {
        return await _dbContext.GetAsync(tableName, id);
    }

    public async Task<bool> UpdateAsync(FormEntity entity)
    {
        return await _dbContext.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(string entityName, string id)
    {
        return await _dbContext.DeleteAsync(entityName, id);
    }
}
