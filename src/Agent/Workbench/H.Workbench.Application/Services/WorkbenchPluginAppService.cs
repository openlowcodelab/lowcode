using System.Text.Json;
using H.Abp.Application.Contracts;
using H.Workbench.Application.Contracts;
using H.Workbench.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Workbench.Application;

/// <summary>
/// 插件（能力包）管理服务实现
/// </summary>
public class WorkbenchPluginAppService : ApplicationService, IWorkbenchPluginAppService
{
    private readonly IRepository<WorkbenchPluginEntity, Guid> _pluginRepository;

    public WorkbenchPluginAppService(IRepository<WorkbenchPluginEntity, Guid> pluginRepository)
    {
        _pluginRepository = pluginRepository;
    }

    public async Task<BaseOutput<PagedResultDto<WorkbenchPluginDto>>> GetListAsync(WorkbenchPluginQueryDto input)
    {
        var query = await _pluginRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.PluginName.Contains(input.Filter) || x.Description.Contains(input.Filter));
        }

        if (!string.IsNullOrWhiteSpace(input.Source))
        {
            query = query.Where(x => x.Source == input.Source);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Source).ThenByDescending(x => x.CreationTime)
                .Skip(input.SkipCount).Take(input.MaxResultCount));

        return new(new PagedResultDto<WorkbenchPluginDto>(totalCount, entities.Select(MapToDto).ToList()));
    }

    public async Task<BaseOutput<WorkbenchPluginDto>> GetAsync(Guid id)
    {
        var entity = await _pluginRepository.FindAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(WorkbenchPluginEntity), id);
        }

        return new(MapToDto(entity));
    }

    [RemoteService(IsEnabled = false)]
    public async Task<BaseOutput<List<WorkbenchPluginDto>>> ListByIdsAsync(List<Guid> ids)
    {
        if (ids.Count == 0)
        {
            return new(new List<WorkbenchPluginDto>());
        }

        var query = await _pluginRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => ids.Contains(x.Id)));
        return new(entities.Select(MapToDto).ToList());
    }

    public async Task<BaseOutput<WorkbenchPluginDto>> CreateAsync(CreateWorkbenchPluginDto input)
    {
        var entity = new WorkbenchPluginEntity
        {
            PluginKey = $"custom-{Guid.NewGuid():N}"[..18],
            PluginName = input.PluginName,
            Description = input.Description,
            Source = "Custom",
            Icon = input.Icon,
            SkillKeys = SerializeKeys(input.SkillKeys),
            IsEnabled = true
        };

        entity = await _pluginRepository.InsertAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<WorkbenchPluginDto>> UpdateAsync(Guid id, UpdateWorkbenchPluginDto input)
    {
        var entity = await _pluginRepository.GetAsync(id);
        entity.PluginName = input.PluginName;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.SkillKeys = SerializeKeys(input.SkillKeys);

        entity = await _pluginRepository.UpdateAsync(entity);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _pluginRepository.GetAsync(id);
        if (entity.Source == "Market")
        {
            throw new InvalidOperationException("插件市场条目不可删除，只能停用");
        }

        await _pluginRepository.DeleteAsync(id);
        return new();
    }

    public async Task<BaseOutput> ToggleEnabledAsync(Guid id, bool isEnabled)
    {
        var entity = await _pluginRepository.GetAsync(id);
        entity.IsEnabled = isEnabled;
        await _pluginRepository.UpdateAsync(entity);
        return new();
    }

    private static string? SerializeKeys(List<string> keys)
    {
        var distinct = keys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return distinct.Count > 0 ? JsonSerializer.Serialize(distinct) : null;
    }

    private static List<string> ParseKeys(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private static WorkbenchPluginDto MapToDto(WorkbenchPluginEntity entity)
    {
        return new WorkbenchPluginDto
        {
            Id = entity.Id,
            PluginKey = entity.PluginKey,
            PluginName = entity.PluginName,
            Description = entity.Description,
            Source = entity.Source,
            Icon = entity.Icon,
            IsEnabled = entity.IsEnabled,
            SkillKeys = ParseKeys(entity.SkillKeys),
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }
}
