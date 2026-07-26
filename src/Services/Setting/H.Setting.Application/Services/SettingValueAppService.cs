using Microsoft.EntityFrameworkCore;
using H.Setting.Application.Contracts;
using H.Setting.EntityFrameworkCore;
using Volo.Abp;
using H.Abp.Application.Contracts;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Setting.Application;

/// <summary>
/// 配置项（配置值）管理服务
/// </summary>
public class SettingValueAppService : ApplicationService, ISettingValueAppService
{
    private readonly IRepository<SettingValue, Guid> _valueRepository;
    private readonly IRepository<SettingDefinition, Guid> _definitionRepository;

    public SettingValueAppService(
        IRepository<SettingValue, Guid> valueRepository,
        IRepository<SettingDefinition, Guid> definitionRepository)
    {
        _valueRepository = valueRepository;
        _definitionRepository = definitionRepository;
    }

    public async Task<PagedResultDto<SettingValueDto>> GetListAsync(SettingValueQueryDto input)
    {
        var query = await _valueRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => x.Name.Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(input.ProviderName))
        {
            query = query.Where(x => x.ProviderName == input.ProviderName);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 10;
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Name).ThenBy(x => x.ProviderName).Skip(input.SkipCount).Take(maxResult));

        // 关联配置定义的显示名称
        var names = entities.Select(x => x.Name).Distinct().ToList();
        var displayNameMap = await AsyncExecuter.ToListAsync(
            (await _definitionRepository.GetQueryableAsync())
                .Where(d => names.Contains(d.Name))
                .Select(d => new { d.Name, d.DisplayName }));
        var map = displayNameMap.ToDictionary(x => x.Name, x => x.DisplayName);

        var dtos = entities.Select(e =>
        {
            var dto = MapToDto(e);
            dto.DisplayName = map.TryGetValue(e.Name, out var dn) ? dn : null;
            return dto;
        }).ToList();

        return new PagedResultDto<SettingValueDto>(totalCount, dtos);
    }

    public async Task<SettingValueDto> GetAsync(Guid id)
    {
        var entity = await _valueRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SettingValueDto> CreateAsync(CreateUpdateSettingValueDto input)
    {
        var name = NormalizeName(input.Name);
        var providerName = NormalizeProviderName(input.ProviderName);

        await CheckDuplicateAsync(name, providerName, input.ProviderKey);

        var entity = new SettingValue
        {
            Name = name,
            Value = input.Value,
            ProviderName = providerName,
            ProviderKey = input.ProviderKey
        };
        await _valueRepository.InsertAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task<SettingValueDto> UpdateAsync(Guid id, CreateUpdateSettingValueDto input)
    {
        var entity = await _valueRepository.GetAsync(id);

        var name = NormalizeName(input.Name);
        var providerName = NormalizeProviderName(input.ProviderName);

        if (!string.Equals(entity.Name, name, StringComparison.Ordinal)
            || !string.Equals(entity.ProviderName, providerName, StringComparison.Ordinal)
            || !string.Equals(entity.ProviderKey, input.ProviderKey, StringComparison.Ordinal))
        {
            await CheckDuplicateAsync(name, providerName, input.ProviderKey, id);
        }

        entity.Name = name;
        entity.Value = input.Value;
        entity.ProviderName = providerName;
        entity.ProviderKey = input.ProviderKey;

        await _valueRepository.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _valueRepository.DeleteAsync(id);
    }

    public async Task<List<SettingDefinitionLookupDto>> GetDefinitionLookupAsync()
    {
        var entities = await AsyncExecuter.ToListAsync(
            (await _definitionRepository.GetQueryableAsync()).OrderBy(x => x.Name));

        return entities.Select(x => new SettingDefinitionLookupDto
        {
            Name = x.Name,
            DisplayName = x.DisplayName,
            DefaultValue = x.DefaultValue
        }).ToList();
    }

    private static string NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new UserFriendlyException("配置名称不能为空");
        }
        return name.Trim();
    }

    private static string NormalizeProviderName(string? providerName)
        => string.IsNullOrWhiteSpace(providerName) ? SettingValueProviders.Global : providerName.Trim();

    private async Task CheckDuplicateAsync(string name, string providerName, string? providerKey, Guid? excludeId = null)
    {
        var exists = await (await _valueRepository.GetQueryableAsync())
            .AnyAsync(x => x.Name == name
                        && x.ProviderName == providerName
                        && x.ProviderKey == providerKey
                        && (excludeId == null || x.Id != excludeId.Value));
        if (exists)
        {
            throw new UserFriendlyException($"配置项 “{name}”（提供者 {providerName}）已存在");
        }
    }

    private static SettingValueDto MapToDto(SettingValue entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Value = entity.Value,
        ProviderName = entity.ProviderName,
        ProviderKey = entity.ProviderKey,
        CreationTime = entity.CreationTime,
        LastModificationTime = entity.LastModificationTime
    };
}
