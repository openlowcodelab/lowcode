using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using H.Abp.Application.Contracts;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using H.Assistant.EntityFrameworkCore;
using H.Assistant.Application.Contracts;

namespace H.Assistant.Application;

/// <summary>
/// Agent 定义管理服务实现
/// </summary>
public class AgentAppService : ApplicationService, IAgentAppService
{
    private readonly IRepository<AgentEntity, Guid> _agentRepository;
    private readonly IRepository<SkillEntity, Guid> _skillRepository;

    public AgentAppService(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
    }

    public async Task<AgentDto> GetAsync(Guid id)
    {
        var entity = await _agentRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<PagedResultDto<AgentDto>> GetListAsync(AgentQueryDto input)
    {
        var query = await _agentRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x =>
                x.AgentType.Contains(input.Filter) ||
                x.DisplayName.Contains(input.Filter) ||
                x.Description.Contains(input.Filter));
        }

        if (input.IsEnabled.HasValue)
        {
            query = query.Where(x => x.IsEnabled == input.IsEnabled.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtos = entities.Select(MapToDto).ToList();

        return new PagedResultDto<AgentDto>(totalCount, dtos);
    }

    public async Task<AgentDto> CreateAsync(CreateAgentDto input)
    {
        // 检查 AgentType 是否已存在
        var query = await _agentRepository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.AgentType == input.AgentType)))
        {
            throw new InvalidOperationException($"Agent 类型 '{input.AgentType}' 已存在");
        }

        var entity = new AgentEntity
        {
            AgentType = input.AgentType,
            DisplayName = input.DisplayName,
            Description = input.Description,
            SystemPrompt = input.SystemPrompt,
            IsEnabled = input.IsEnabled,
            SupportsStreaming = input.SupportsStreaming,
            Temperature = input.Temperature,
            MaxTokens = input.MaxTokens,
            DefaultModelConfigId = input.DefaultModelConfigId,
            Metadata = input.Metadata,
            SkillIds = input.SkillIds.Any() ? JsonSerializer.Serialize(input.SkillIds) : null
        };

        await _agentRepository.InsertAsync(entity);
        return MapToDto(entity);
    }

    public async Task<AgentDto> UpdateAsync(Guid id, UpdateAgentDto input)
    {
        var entity = await _agentRepository.GetAsync(id);

        entity.DisplayName = input.DisplayName;
        entity.Description = input.Description;
        entity.SystemPrompt = input.SystemPrompt;
        entity.IsEnabled = input.IsEnabled;
        entity.SupportsStreaming = input.SupportsStreaming;
        entity.Temperature = input.Temperature;
        entity.MaxTokens = input.MaxTokens;
        entity.DefaultModelConfigId = input.DefaultModelConfigId;
        entity.Metadata = input.Metadata;
        entity.SkillIds = input.SkillIds.Any() ? JsonSerializer.Serialize(input.SkillIds) : null;

        await _agentRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _agentRepository.DeleteAsync(id);
    }

    public async Task ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _agentRepository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _agentRepository.UpdateAsync(entity);
    }

    public async Task<List<AgentDto>> GetEnabledAgentsAsync()
    {
        var query = await _agentRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.IsEnabled));
        return entities.Select(MapToDto).ToList();
    }

    public async Task AddSkillAsync(Guid agentId, Guid skillId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (!skillIds.Contains(skillId))
        {
            skillIds.Add(skillId);
            agent.SkillIds = JsonSerializer.Serialize(skillIds);
            await _agentRepository.UpdateAsync(agent);
        }
    }

    public async Task RemoveSkillAsync(Guid agentId, Guid skillId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (skillIds.Remove(skillId))
        {
            agent.SkillIds = skillIds.Any() ? JsonSerializer.Serialize(skillIds) : null;
            await _agentRepository.UpdateAsync(agent);
        }
    }

    public async Task<List<SkillDto>> GetAgentSkillsAsync(Guid agentId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (!skillIds.Any())
        {
            return new List<SkillDto>();
        }

        var query = await _skillRepository.GetQueryableAsync();
        var skills = await AsyncExecuter.ToListAsync(query.Where(x => skillIds.Contains(x.Id)));
        return skills.Select(MapSkillToDto).ToList();
    }

    private static List<Guid> GetSkillIds(AgentEntity agent)
    {
        if (string.IsNullOrWhiteSpace(agent.SkillIds))
        {
            return new List<Guid>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(agent.SkillIds) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    private static AgentDto MapToDto(AgentEntity entity)
    {
        var skillIds = string.IsNullOrWhiteSpace(entity.SkillIds)
            ? new List<Guid>()
            : JsonSerializer.Deserialize<List<Guid>>(entity.SkillIds) ?? new List<Guid>();

        return new AgentDto
        {
            Id = entity.Id,
            AgentType = entity.AgentType,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            SystemPrompt = entity.SystemPrompt,
            IsEnabled = entity.IsEnabled,
            SupportsStreaming = entity.SupportsStreaming,
            Temperature = entity.Temperature,
            MaxTokens = entity.MaxTokens,
            DefaultModelConfigId = entity.DefaultModelConfigId,
            Metadata = entity.Metadata,
            Skills = skillIds.Select(x => x.ToString()).ToList(),
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeletionTime = entity.DeletionTime,
            DeleterId = entity.DeleterId
        };
    }

    private static SkillDto MapSkillToDto(SkillEntity entity)
    {
        return new SkillDto
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
