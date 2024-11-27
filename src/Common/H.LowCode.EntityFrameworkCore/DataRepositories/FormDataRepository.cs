using H.LowCode.Domain;
using H.LowCode.MetaSchema;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace H.LowCode.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    private LowCodeDbContext _dbContext;
    public bool? IsChangeTrackingEnabled => true;

    public FormDataRepository(LowCodeDbContext dbContext)
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
