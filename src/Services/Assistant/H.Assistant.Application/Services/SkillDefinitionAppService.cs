using Microsoft.EntityFrameworkCore;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using H.Assistant.EntityFrameworkCore;
using H.Assistant.Application.Contracts;

namespace H.Assistant.Application;

/// <summary>
/// 技能定义管理服务实现
/// </summary>
public class SkillDefinitionAppService : ApplicationService, ISkillDefinitionAppService
{
    private readonly IRepository<SkillDefinitionEntity, Guid> _skillRepository;

    public SkillDefinitionAppService(IRepository<SkillDefinitionEntity, Guid> skillRepository)
    {
        _skillRepository = skillRepository;
    }

    public async Task<SkillDefinitionDto> GetAsync(Guid id)
    {
        var entity = await _skillRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<PagedResultDto<SkillDefinitionDto>> GetListAsync(SkillDefinitionQueryDto input)
    {
        var query = await _skillRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x =>
                x.SkillName.Contains(input.Filter) ||
                x.DisplayName.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.SkillType))
        {
            query = query.Where(x => x.SkillType == input.SkillType);
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).PageBy(input));
        var dtos = entities.Select(MapToDto).ToList();

        return new PagedResultDto<SkillDefinitionDto>(totalCount, dtos);
    }

    public async Task<SkillDefinitionDto> CreateAsync(CreateSkillDefinitionDto input)
    {
        // 检查技能名称是否已存在
        var query = await _skillRepository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.SkillName == input.SkillName)))
        {
            throw new InvalidOperationException($"技能 '{input.SkillName}' 已存在");
        }

        var entity = new SkillDefinitionEntity
        {
            SkillName = input.SkillName,
            DisplayName = input.DisplayName,
            Description = input.Description,
            SkillType = input.SkillType,
            ImplementationClass = input.ImplementationClass,
            Config = input.Config,
            ParameterSchema = input.ParameterSchema,
            IsEnabled = input.IsEnabled,
            RequiresApproval = input.RequiresApproval,
            UsageCount = 0
        };

        await _skillRepository.InsertAsync(entity);
        return MapToDto(entity);
    }

    public async Task<SkillDefinitionDto> UpdateAsync(Guid id, UpdateSkillDefinitionDto input)
    {
        var entity = await _skillRepository.GetAsync(id);

        entity.DisplayName = input.DisplayName;
        entity.Description = input.Description;
        entity.ImplementationClass = input.ImplementationClass;
        entity.Config = input.Config;
        entity.ParameterSchema = input.ParameterSchema;
        entity.IsEnabled = input.IsEnabled;
        entity.RequiresApproval = input.RequiresApproval;

        await _skillRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _skillRepository.DeleteAsync(id);
    }

    public async Task ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _skillRepository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _skillRepository.UpdateAsync(entity);
    }

    public async Task<List<SkillDefinitionDto>> GetEnabledSkillsAsync()
    {
        var query = await _skillRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.IsEnabled));
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<SkillDefinitionDto>> GetSkillsByTypeAsync(string skillType)
    {
        var query = await _skillRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.SkillType == skillType && x.IsEnabled));
        return entities.Select(MapToDto).ToList();
    }

    public async Task IncrementUsageCountAsync(Guid id)
    {
        var entity = await _skillRepository.GetAsync(id);
        entity.UsageCount++;
        entity.LastUsedTime = DateTime.UtcNow;
        await _skillRepository.UpdateAsync(entity);
    }

    private static SkillDefinitionDto MapToDto(SkillDefinitionEntity entity)
    {
        return new SkillDefinitionDto
        {
            Id = entity.Id,
            SkillName = entity.SkillName,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            SkillType = entity.SkillType,
            ImplementationClass = entity.ImplementationClass,
            Config = entity.Config,
            ParameterSchema = entity.ParameterSchema,
            IsEnabled = entity.IsEnabled,
            RequiresApproval = entity.RequiresApproval,
            UsageCount = entity.UsageCount,
            LastUsedTime = entity.LastUsedTime,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeletionTime = entity.DeletionTime,
            DeleterId = entity.DeleterId
        };
    }
}
