using H.LowCode.Entity;
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
    private readonly IDbContextFactory<RenderEngineDbContext> _dbContextFactory;
    public bool? IsChangeTrackingEnabled => true;

    public FormDataRepository(IDbContextFactory<RenderEngineDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<bool> AddAsync(FormEntity entity)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.AddAsync(entity);
    }

    public async Task<FormEntity> GetAsync(string tableName, string id)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.GetAsync(tableName, id);
    }

    public async Task<bool> UpdateAsync(FormEntity entity)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(string entityName, string id)
    {
        using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.DeleteAsync(entityName, id);
    }
}
