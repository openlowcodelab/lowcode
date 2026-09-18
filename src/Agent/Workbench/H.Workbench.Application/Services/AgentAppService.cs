using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// Agent 定义管理服务实现
/// </summary>
public class AgentAppService : ApplicationService, IAgentAppService
{
    private readonly IRepository<AgentEntity, Guid> _agentRepository;
    private readonly IRepository<SkillEntity, Guid> _skillRepository;
    private readonly IRepository<AgentTemplateEntity, Guid> _templateRepository;

    public AgentAppService(
        IRepository<AgentEntity, Guid> agentRepository,
        IRepository<SkillEntity, Guid> skillRepository,
        IRepository<AgentTemplateEntity, Guid> templateRepository)
    {
        _agentRepository = agentRepository;
        _skillRepository = skillRepository;
        _templateRepository = templateRepository;
    }

    public async Task<BaseOutput<AgentDto>> GetAsync(Guid id)
    {
        var entity = await _agentRepository.GetAsync(id);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<PagedResultDto<AgentDto>>> GetListAsync(AgentQueryDto input)
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

        return new(new PagedResultDto<AgentDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<AgentDto>> CreateAsync(CreateAgentDto input)
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
            Role = input.Role,
            SkillIds = SerializeIds(input.SkillIds),
            ConnectorIds = SerializeIds(input.ConnectorIds),
            KnowledgeBaseIds = SerializeIds(input.KnowledgeBaseIds),
            ProjectIds = SerializeIds(input.ProjectIds)
        };

        entity = await _agentRepository.InsertAsync(entity);

        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<AgentDto>> UpdateAsync(Guid id, UpdateAgentDto input)
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
        entity.Role = input.Role;
        entity.SkillIds = SerializeIds(input.SkillIds);
        entity.ConnectorIds = SerializeIds(input.ConnectorIds);
        entity.KnowledgeBaseIds = SerializeIds(input.KnowledgeBaseIds);
        entity.ProjectIds = SerializeIds(input.ProjectIds);

        entity = await _agentRepository.UpdateAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _agentRepository.DeleteAsync(id);
        return new();
    }

    public async Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _agentRepository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _agentRepository.UpdateAsync(entity);
        return new();
    }

    public async Task<BaseOutput<List<AgentDto>>> GetEnabledAgentsAsync()
    {
        var query = await _agentRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.IsEnabled));
        return new(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput> AddSkillAsync(Guid agentId, Guid skillId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (!skillIds.Contains(skillId))
        {
            skillIds.Add(skillId);
            agent.SkillIds = JsonSerializer.Serialize(skillIds);
            await _agentRepository.UpdateAsync(agent);
        }

        return new();
    }

    public async Task<BaseOutput> RemoveSkillAsync(Guid agentId, Guid skillId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (skillIds.Remove(skillId))
        {
            agent.SkillIds = skillIds.Any() ? JsonSerializer.Serialize(skillIds) : null;
            await _agentRepository.UpdateAsync(agent);
        }

        return new();
    }

    public async Task<BaseOutput<List<SkillDto>>> GetAgentSkillsAsync(Guid agentId)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var skillIds = GetSkillIds(agent);

        if (!skillIds.Any())
        {
            return new(new List<SkillDto>());
        }

        var query = await _skillRepository.GetQueryableAsync();
        var skills = await AsyncExecuter.ToListAsync(query.Where(x => skillIds.Contains(x.Id)));
        return new(skills.Select(MapSkillToDto).ToList());
    }

    public async Task<BaseOutput<AgentDto>> CreateFromTemplateAsync(CreateAgentFromTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(input.TemplateId);

        var query = await _agentRepository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.DisplayName == input.DisplayName)))
        {
            throw new InvalidOperationException($"员工 '{input.DisplayName}' 已存在");
        }

        var entity = new AgentEntity
        {
            AgentType = $"waker-{Guid.NewGuid():N}"[..18],
            DisplayName = input.DisplayName,
            Description = template.Description,
            SystemPrompt = template.SystemPrompt,
            IsEnabled = true,
            Role = string.IsNullOrWhiteSpace(input.Role) ? template.Role : input.Role,
            SkillIds = template.SkillIds,
            ConnectorIds = template.ConnectorIds,
            KnowledgeBaseIds = template.KnowledgeBaseIds,
            ProjectIds = template.ProjectIds
        };

        entity = await _agentRepository.InsertAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<AgentTemplateDto>> SaveAsTemplateAsync(SaveAgentTemplateDto input)
    {
        var agent = await _agentRepository.GetAsync(input.AgentId);

        var entity = new AgentTemplateEntity
        {
            TemplateName = input.TemplateName,
            Role = agent.Role,
            Description = agent.Description,
            SystemPrompt = agent.SystemPrompt,
            SkillIds = agent.SkillIds,
            ConnectorIds = agent.ConnectorIds,
            KnowledgeBaseIds = agent.KnowledgeBaseIds,
            ProjectIds = agent.ProjectIds,
            IsBuiltin = false
        };

        entity = await _templateRepository.InsertAsync(entity);
        return new(MapTemplateToDto(entity));
    }

    private static string? SerializeIds(List<Guid> ids)
        => ids.Any() ? JsonSerializer.Serialize(ids) : null;

    private static List<Guid> ParseIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Guid>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(json) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    private static AgentTemplateDto MapTemplateToDto(AgentTemplateEntity entity)
    {
        return new AgentTemplateDto
        {
            Id = entity.Id,
            TemplateName = entity.TemplateName,
            Role = entity.Role,
            Description = entity.Description,
            SystemPrompt = entity.SystemPrompt,
            SkillIds = ParseIds(entity.SkillIds),
            ConnectorIds = ParseIds(entity.ConnectorIds),
            KnowledgeBaseIds = ParseIds(entity.KnowledgeBaseIds),
            ProjectIds = ParseIds(entity.ProjectIds),
            IsBuiltin = entity.IsBuiltin,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    private static List<Guid> GetSkillIds(AgentEntity agent) => ParseIds(agent.SkillIds);

    private static AgentDto MapToDto(AgentEntity entity)
    {
        var skillIds = ParseIds(entity.SkillIds);

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
            Role = entity.Role,
            Skills = skillIds.Select(x => x.ToString()).ToList(),
            SkillIdList = skillIds,
            ConnectorIds = ParseIds(entity.ConnectorIds),
            KnowledgeBaseIds = ParseIds(entity.KnowledgeBaseIds),
            ProjectIds = ParseIds(entity.ProjectIds),
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
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
            LastModifierId = entity.LastModifierId
        };
    }
}
