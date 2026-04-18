using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using H.Approval.Application.Contracts;
using H.Approval.EntityFrameworkCore.Entities;
using H.Approval.EntityFrameworkCore.Repositories;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;

namespace H.Approval.Application.Services;

/// <summary>
/// 审批定义应用服务
/// </summary>
public class ApprovalDefinitionAppService : ApplicationService, IApprovalDefinitionService
{
    private readonly ILogger<ApprovalDefinitionAppService> _logger;
    private readonly IApprovalDefinitionRepository _definitionRepository;

    public ApprovalDefinitionAppService(
        ILogger<ApprovalDefinitionAppService> logger,
        IApprovalDefinitionRepository definitionRepository)
    {
        _logger = logger;
        _definitionRepository = definitionRepository;
    }
    
    public async Task<List<ApprovalDefinitionDto>> GetAllAsync()
    {
        _logger.LogInformation("获取所有审批定义");
        var entities = await _definitionRepository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<ApprovalDefinitionDto> GetByIdAsync(string id)
    {
        _logger.LogInformation("获取审批定义: Id={Id}", id);
        
        var entity = await _definitionRepository.GetByIdAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批定义不存在: {id}");
        }
        
        return MapToDto(entity);
    }

    public async Task<ApprovalDefinitionDto> CreateAsync(CreateApprovalDefinitionDto input)
    {
        _logger.LogInformation("创建审批定义: Name={Name}", input.Name);
        
        var id = Guid.NewGuid().ToString();
        var entity = new ApprovalDefinition(id)
        {
            Name = input.Name,
            Description = input.Description,
            DefinitionJson = input.DefinitionJson,
            Version = 1,
            IsEnabled = true,
            CreationTime = DateTime.Now
        };
        
        await _definitionRepository.InsertAsync(entity);
        
        _logger.LogInformation("审批定义已创建: Id={Id}, Name={Name}", entity.Id, input.Name);
        
        return MapToDto(entity);
    }

    public async Task<ApprovalDefinitionDto> UpdateAsync(UpdateApprovalDefinitionDto input)
    {
        _logger.LogInformation("更新审批定义: Id={Id}", input.Id);
        
        var entity = await _definitionRepository.GetByIdAsync(input.Id);
        if (entity == null)
        {
            throw new KeyNotFoundException($"审批定义不存在: {input.Id}");
        }
        
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.DefinitionJson = input.DefinitionJson;
        entity.LastModificationTime = DateTime.Now;
        
        await _definitionRepository.UpdateAsync(entity);
        
        _logger.LogInformation("审批定义已更新: Id={Id}", input.Id);
        
        return MapToDto(entity);
    }

    public async Task DeleteAsync(string id)
    {
        _logger.LogInformation("删除审批定义: Id={Id}", id);
        
        await _definitionRepository.DeleteAsync(id);
        
        _logger.LogInformation("审批定义已删除: Id={Id}", id);
    }

    public async Task ToggleEnabledAsync(string id, bool enabled)
    {
        _logger.LogInformation("{Action}审批定义: Id={Id}", enabled ? "启用" : "禁用", id);
        
        var entity = await _definitionRepository.GetByIdAsync(id);
        if (entity != null)
        {
            entity.IsEnabled = enabled;
            entity.LastModificationTime = DateTime.Now;
            await _definitionRepository.UpdateAsync(entity);
            
            _logger.LogInformation("审批定义已{Action}: Id={Id}", enabled ? "启用" : "禁用", id);
        }
    }
    
    private static ApprovalDefinitionDto MapToDto(ApprovalDefinition entity)
    {
        return new ApprovalDefinitionDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            DefinitionJson = entity.DefinitionJson,
            Version = entity.Version,
            IsEnabled = entity.IsEnabled,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}
