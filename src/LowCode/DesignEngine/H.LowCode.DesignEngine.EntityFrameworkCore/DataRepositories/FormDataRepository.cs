using H.LowCode.DesignEngine.Domain;
using H.LowCode.Entity;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class FormDataRepository : IFormDataRepository
{
    private DesignEngineDbContext _dbContext;
    public bool? IsChangeTrackingEnabled => true;

    public string? EntityName { get; set; }

    public string ProviderName => throw new NotImplementedException();

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
        return _dbContext.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(string entityName, string id)
    {
        return _dbContext.DeleteAsync(entityName, id);
    }
}
