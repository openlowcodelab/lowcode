using Microsoft.EntityFrameworkCore;
using H.Setting.Application.Contracts;
using H.Setting.EntityFrameworkCore;
using Volo.Abp;
using H.Abp.Application.Contracts;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Setting.Application;

/// <summary>
/// 配置定义管理服务
/// </summary>
public class SettingDefinitionAppService : ApplicationService, ISettingDefinitionAppService
{
    private readonly IRepository<SettingDefinition, Guid> _definitionRepository;
    private readonly IRepository<SettingValue, Guid> _valueRepository;

    public SettingDefinitionAppService(
        IRepository<SettingDefinition, Guid> definitionRepository,
        IRepository<SettingValue, Guid> valueRepository)
    {
        _definitionRepository = definitionRepository;
        _valueRepository = valueRepository;
    }

    public async Task<PagedResultDto<SettingDefinitionDto>> GetListAsync(SettingDefinitionQueryDto input)
    {
        var query = await _definitionRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => x.Name.Contains(filter) || x.DisplayName.Contains(filter));
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Name).Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(MapToDto).ToList();
        return new PagedResultDto<SettingDefinitionDto>(totalCount, dtos);
    }

    public async Task<SettingDefinitionDto> GetAsync(Guid id)
    {
        var entity = await _definitionRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SettingDefinitionDto> CreateAsync(CreateUpdateSettingDefinitionDto input)
    {
        await CheckNameDuplicateAsync(input.Name);

        var entity = new SettingDefinition
        {
            Name = input.Name.Trim(),
            DisplayName = input.DisplayName,
            Description = input.Description,
            DefaultValue = input.DefaultValue,
            IsVisibleToClients = input.IsVisibleToClients,
            Providers = input.Providers,
            IsInherited = input.IsInherited,
            IsEncrypted = input.IsEncrypted
        };
        await _definitionRepository.InsertAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task<SettingDefinitionDto> UpdateAsync(Guid id, CreateUpdateSettingDefinitionDto input)
    {
        var entity = await _definitionRepository.GetAsync(id);

        if (!string.Equals(entity.Name, input.Name?.Trim(), StringComparison.Ordinal))
        {
            await CheckNameDuplicateAsync(input.Name, id);
            entity.Name = input.Name!.Trim();
        }

        entity.DisplayName = input.DisplayName;
        entity.Description = input.Description;
        entity.DefaultValue = input.DefaultValue;
        entity.IsVisibleToClients = input.IsVisibleToClients;
        entity.Providers = input.Providers;
        entity.IsInherited = input.IsInherited;
        entity.IsEncrypted = input.IsEncrypted;

        await _definitionRepository.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _definitionRepository.GetAsync(id);
        var hasValues = await (await _valueRepository.GetQueryableAsync()).AnyAsync(x => x.Name == entity.Name);
        if (hasValues)
        {
            throw new UserFriendlyException("该配置定义下存在配置项，无法删除");
        }
        await _definitionRepository.DeleteAsync(id);
    }

    private async Task CheckNameDuplicateAsync(string? name, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new UserFriendlyException("配置名称不能为空");
        }

        var trimmed = name.Trim();
        var exists = await (await _definitionRepository.GetQueryableAsync())
            .AnyAsync(x => x.Name == trimmed && (excludeId == null || x.Id != excludeId.Value));
        if (exists)
        {
            throw new UserFriendlyException($"配置名称 “{trimmed}” 已存在");
        }
    }

    private static SettingDefinitionDto MapToDto(SettingDefinition entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        DisplayName = entity.DisplayName,
        Description = entity.Description,
        DefaultValue = entity.DefaultValue,
        IsVisibleToClients = entity.IsVisibleToClients,
        Providers = entity.Providers,
        IsInherited = entity.IsInherited,
        IsEncrypted = entity.IsEncrypted,
        CreationTime = entity.CreationTime,
        LastModificationTime = entity.LastModificationTime
    };
}
