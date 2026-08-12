using Microsoft.EntityFrameworkCore;

namespace H.Approval.EntityFrameworkCore;

/// <summary>
/// 审批定义仓储实现
/// </summary>
public class ApprovalDefinitionRepository : IApprovalDefinitionRepository
{
    private readonly ApprovalDbContext _dbContext;

    public ApprovalDefinitionRepository(ApprovalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ApprovalDefinition>> GetAllAsync()
    {
        return await _dbContext.ApprovalDefinitions
            .OrderByDescending(d => d.CreationTime)
            .ToListAsync();
    }

    public async Task<ApprovalDefinition?> GetByIdAsync(string id)
    {
        return await _dbContext.ApprovalDefinitions.FindAsync(id);
    }

    public async Task<ApprovalDefinition> InsertAsync(ApprovalDefinition entity)
    {
        await _dbContext.ApprovalDefinitions.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ApprovalDefinition> UpdateAsync(ApprovalDefinition entity)
    {
        _dbContext.ApprovalDefinitions.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _dbContext.ApprovalDefinitions.FindAsync(id);
        if (entity != null)
        {
            _dbContext.ApprovalDefinitions.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}

/// <summary>
/// 审批实例仓储实现
/// </summary>
public class ApprovalInstanceRepository : IApprovalInstanceRepository
{
    private readonly ApprovalDbContext _dbContext;

    public ApprovalInstanceRepository(ApprovalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApprovalInstance> InsertAsync(ApprovalInstance entity)
    {
        await _dbContext.ApprovalInstances.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ApprovalInstance?> GetByIdAsync(string id)
    {
        return await _dbContext.ApprovalInstances
            .Include(i => i.Tasks)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<ApprovalInstance>> GetByCreatorIdAsync(string creatorId)
    {
        return await _dbContext.ApprovalInstances
            .Where(i => i.CreatorId == creatorId)
            .OrderByDescending(i => i.CreationTime)
            .ToListAsync();
    }

    public async Task<ApprovalInstance> UpdateAsync(ApprovalInstance entity)
    {
        _dbContext.ApprovalInstances.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }
}

/// <summary>
/// 审批任务仓储实现
/// </summary>
public class ApprovalTaskRepository : IApprovalTaskRepository
{
    private readonly ApprovalDbContext _dbContext;

    public ApprovalTaskRepository(ApprovalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApprovalTask> InsertAsync(ApprovalTask entity)
    {
        await _dbContext.ApprovalTasks.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<List<ApprovalTask>> GetPendingByAssigneeIdAsync(string assigneeId)
    {
        return await _dbContext.ApprovalTasks
            .Where(t => t.AssigneeId == assigneeId && t.Status == 0)
            .OrderByDescending(t => t.CreationTime)
            .ToListAsync();
    }

    public async Task<List<ApprovalTask>> GetCompletedByAssigneeIdAsync(string assigneeId)
    {
        return await _dbContext.ApprovalTasks
            .Where(t => t.AssigneeId == assigneeId && t.Status != 0)
            .OrderByDescending(t => t.ApprovalTime)
            .ToListAsync();
    }

    public async Task<List<ApprovalTask>> GetByInstanceIdAsync(string instanceId)
    {
        return await _dbContext.ApprovalTasks
            .Where(t => t.InstanceId == instanceId)
            .OrderBy(t => t.CreationTime)
            .ToListAsync();
    }

    public async Task<List<ApprovalTask>> GetByNodeIdAsync(string instanceId, string nodeId)
    {
        return await _dbContext.ApprovalTasks
            .Where(t => t.InstanceId == instanceId && t.NodeId == nodeId)
            .OrderBy(t => t.CreationTime)
            .ToListAsync();
    }

    public async Task<ApprovalTask?> GetByIdAsync(string id)
    {
        return await _dbContext.ApprovalTasks.FindAsync(id);
    }

    public async Task<ApprovalTask> UpdateAsync(ApprovalTask entity)
    {
        _dbContext.ApprovalTasks.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateRangeAsync(List<ApprovalTask> entities)
    {
        _dbContext.ApprovalTasks.UpdateRange(entities);
        await _dbContext.SaveChangesAsync();
    }
}

/// <summary>
/// 审批分类(分组)仓储实现
/// </summary>
public class ApprovalCategoryRepository : IApprovalCategoryRepository
{
    private readonly ApprovalDbContext _dbContext;

    public ApprovalCategoryRepository(ApprovalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ApprovalCategory>> GetAllAsync()
    {
        return await _dbContext.ApprovalCategories
            .OrderBy(c => c.Sort)
            .ThenBy(c => c.CreationTime)
            .ToListAsync();
    }

    public async Task<ApprovalCategory?> GetByIdAsync(string id)
    {
        return await _dbContext.ApprovalCategories.FindAsync(id);
    }

    public async Task<ApprovalCategory?> GetByNameAsync(string name)
    {
        return await _dbContext.ApprovalCategories
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<ApprovalCategory> InsertAsync(ApprovalCategory entity)
    {
        await _dbContext.ApprovalCategories.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task<ApprovalCategory> UpdateAsync(ApprovalCategory entity)
    {
        _dbContext.ApprovalCategories.Update(entity);
        await _dbContext.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _dbContext.ApprovalCategories.FindAsync(id);
        if (entity != null)
        {
            _dbContext.ApprovalCategories.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
