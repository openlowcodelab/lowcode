using H.Abp.Application.Contracts;
using H.Assistant.Application.Contracts;
using H.Assistant.EntityFrameworkCore;
using H.Util.Base;
using System.Text.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Assistant.Application;

/// <summary>
/// 员工模板管理服务实现
/// </summary>
public class AgentTemplateAppService : ApplicationService, IAgentTemplateAppService
{
    private readonly IRepository<AgentTemplateEntity, Guid> _templateRepository;

    public AgentTemplateAppService(IRepository<AgentTemplateEntity, Guid> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<BaseOutput<PagedResultDto<AgentTemplateDto>>> GetListAsync(AgentTemplateQueryDto input)
    {
        var query = await _templateRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.TemplateName.Contains(input.Filter) || x.Role.Contains(input.Filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.IsBuiltin).ThenByDescending(x => x.CreationTime)
                .Skip(input.SkipCount).Take(input.MaxResultCount));

        return new(new PagedResultDto<AgentTemplateDto>(totalCount, entities.Select(MapToDto).ToList()));
    }

    public async Task<BaseOutput<AgentTemplateDto>> GetAsync(Guid id)
    {
        var entity = await _templateRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(AgentTemplateEntity), id);
        }

        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _templateRepository.GetAsync(id);
        if (entity.IsBuiltin)
        {
            throw new InvalidOperationException("内置模板不可删除");
        }

        await _templateRepository.DeleteAsync(id);
        return new();
    }

    private static AgentTemplateDto MapToDto(AgentTemplateEntity entity)
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
}
