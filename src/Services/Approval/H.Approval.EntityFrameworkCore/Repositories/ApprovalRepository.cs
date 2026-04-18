using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H.Approval.EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace H.Approval.EntityFrameworkCore.Repositories;

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
}
