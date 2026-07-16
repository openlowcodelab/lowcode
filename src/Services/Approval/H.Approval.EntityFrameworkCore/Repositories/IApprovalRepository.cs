using System;

namespace H.Approval.EntityFrameworkCore;

/// <summary>
/// 审批定义仓储接口
/// </summary>
public interface IApprovalDefinitionRepository
{
    Task<List<ApprovalDefinition>> GetAllAsync();
    Task<ApprovalDefinition?> GetByIdAsync(string id);
    Task<ApprovalDefinition> InsertAsync(ApprovalDefinition entity);
    Task<ApprovalDefinition> UpdateAsync(ApprovalDefinition entity);
    Task DeleteAsync(string id);
}

/// <summary>
/// 审批实例仓储接口
/// </summary>
public interface IApprovalInstanceRepository
{
    Task<ApprovalInstance> InsertAsync(ApprovalInstance entity);
    Task<ApprovalInstance?> GetByIdAsync(string id);
    Task<List<ApprovalInstance>> GetByCreatorIdAsync(string creatorId);
    Task<ApprovalInstance> UpdateAsync(ApprovalInstance entity);
}

/// <summary>
/// 审批任务仓储接口
/// </summary>
public interface IApprovalTaskRepository
{
    Task<ApprovalTask> InsertAsync(ApprovalTask entity);
    Task<List<ApprovalTask>> GetPendingByAssigneeIdAsync(string assigneeId);
    Task<List<ApprovalTask>> GetCompletedByAssigneeIdAsync(string assigneeId);
    Task<List<ApprovalTask>> GetByInstanceIdAsync(string instanceId);
    Task<List<ApprovalTask>> GetByNodeIdAsync(string instanceId, string nodeId);
    Task<ApprovalTask?> GetByIdAsync(string id);
    Task<ApprovalTask> UpdateAsync(ApprovalTask entity);
    Task UpdateRangeAsync(List<ApprovalTask> entities);
}

/// <summary>
/// 审批分类(分组)仓储接口
/// </summary>
public interface IApprovalCategoryRepository
{
    Task<List<ApprovalCategory>> GetAllAsync();
    Task<ApprovalCategory?> GetByIdAsync(string id);
    Task<ApprovalCategory?> GetByNameAsync(string name);
    Task<ApprovalCategory> InsertAsync(ApprovalCategory entity);
    Task<ApprovalCategory> UpdateAsync(ApprovalCategory entity);
    Task DeleteAsync(string id);
}
