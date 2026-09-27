using H.Abp.Application.Contracts;
using H.Util.Base;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace H.Workbench.Application;

/// <summary>
/// 审批规则管理：规则在运行时由 AgentFactory 取出、ReactAgent 按工具+参数逐次判定。
/// </summary>
public class ApprovalRuleAppService : ApplicationService, IApprovalRuleAppService
{
    private static readonly string[] AllowedEffects = ["Require", "Auto", "Deny"];

    private readonly IRepository<ApprovalRuleEntity, Guid> _repository;

    public ApprovalRuleAppService(IRepository<ApprovalRuleEntity, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<BaseOutput<List<ApprovalRuleDto>>> GetListAsync(string? agentType = null)
    {
        var queryable = await _repository.GetQueryableAsync();
        var query = queryable.Where(x => x.IsEnabled);
        if (!string.IsNullOrWhiteSpace(agentType))
        {
            query = query.Where(x => x.AgentType == null || x.AgentType == "" || x.AgentType == agentType);
        }

        var rules = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.Priority));
        return new(rules.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput<List<ApprovalRuleDto>>> GetEffectiveRulesAsync(string? agentType)
        => await GetListAsync(agentType);

    public async Task<BaseOutput<ApprovalRuleDto>> CreateAsync(CreateApprovalRuleDto input)
    {
        Validate(input);

        var entity = new ApprovalRuleEntity
        {
            Name = input.Name.Trim(),
            AgentType = string.IsNullOrWhiteSpace(input.AgentType) ? null : input.AgentType.Trim(),
            ToolPattern = input.ToolPattern.Trim(),
            ArgPattern = string.IsNullOrWhiteSpace(input.ArgPattern) ? null : input.ArgPattern.Trim(),
            Effect = input.Effect,
            Priority = input.Priority,
            IsEnabled = input.IsEnabled
        };

        entity = await _repository.InsertAsync(entity, autoSave: true);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<ApprovalRuleDto>> UpdateAsync(Guid id, UpdateApprovalRuleDto input)
    {
        Validate(input);

        var entity = await _repository.FindAsync(id);
        if (entity == null) throw new EntityNotFoundException(typeof(ApprovalRuleEntity), id);

        entity.Name = input.Name.Trim();
        entity.AgentType = string.IsNullOrWhiteSpace(input.AgentType) ? null : input.AgentType.Trim();
        entity.ToolPattern = input.ToolPattern.Trim();
        entity.ArgPattern = string.IsNullOrWhiteSpace(input.ArgPattern) ? null : input.ArgPattern.Trim();
        entity.Effect = input.Effect;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;

        await _repository.UpdateAsync(entity, autoSave: true);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
        return new();
    }

    /// <summary>
    /// 参数正则在建规则时就校验：写坏的正则会在每次工具调用处静默不匹配，
    /// 表现为"该批的没批"，比直接报错危险得多
    /// </summary>
    private static void Validate(CreateApprovalRuleDto input)
    {
        if (!AllowedEffects.Contains(input.Effect, StringComparer.OrdinalIgnoreCase))
        {
            throw new AbpValidationException($"审批规则效果只能是 {string.Join('/', AllowedEffects)}",
                [new System.ComponentModel.DataAnnotations.ValidationResult(
                    $"Effect 无效: {input.Effect}", new[] { nameof(input.Effect) })]);
        }

        if (!string.IsNullOrWhiteSpace(input.ArgPattern))
        {
            try
            {
                _ = new Regex(input.ArgPattern, RegexOptions.None, TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException ex)
            {
                throw new AbpValidationException($"参数正则无法编译: {ex.Message}",
                    [new System.ComponentModel.DataAnnotations.ValidationResult(
                        "ArgPattern 不是合法正则", new[] { nameof(input.ArgPattern) })]);
            }
        }
    }

    private static ApprovalRuleDto MapToDto(ApprovalRuleEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        AgentType = e.AgentType,
        ToolPattern = e.ToolPattern,
        ArgPattern = e.ArgPattern,
        Effect = e.Effect,
        Priority = e.Priority,
        IsEnabled = e.IsEnabled,
        CreationTime = e.CreationTime
    };
}
