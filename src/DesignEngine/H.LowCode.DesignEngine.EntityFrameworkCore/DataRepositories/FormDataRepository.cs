using H.LowCode.DesignEngine.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    private DesignEngineDbContext _dbContext;
    public bool? IsChangeTrackingEnabled => true;

    public FormDataRepository(DesignEngineDbContext dbContext)
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

    public Task<bool> UpdateAsync(FormEntity entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(string entityName, string id)
    {
        throw new NotImplementedException();
    }
}
