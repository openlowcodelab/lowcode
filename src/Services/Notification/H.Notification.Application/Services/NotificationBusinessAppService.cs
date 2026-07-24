using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class NotificationBusinessAppService : ApplicationService, INotificationBusinessAppService
{
    private static readonly Regex CodeSuffixRegex = new("^[a-z]{3,16}$", RegexOptions.Compiled);

    private readonly IRepository<NotificationBusinessEntity, Guid> _businessRepository;
    private readonly IRepository<NotificationSpecEntity, Guid> _specRepository;
    private readonly IRepository<NotificationBusinessGroupEntity, Guid> _groupBindingRepository;
    private readonly IRepository<NotificationCategory, long> _categoryRepository;

    public NotificationBusinessAppService(
        IRepository<NotificationBusinessEntity, Guid> businessRepository,
        IRepository<NotificationSpecEntity, Guid> specRepository,
        IRepository<NotificationBusinessGroupEntity, Guid> groupBindingRepository,
        IRepository<NotificationCategory, long> categoryRepository)
    {
        _businessRepository = businessRepository;
        _specRepository = specRepository;
        _groupBindingRepository = groupBindingRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResultDto<NotificationBusinessDto>> GetListAsync(NotificationBusinessQueryDto input)
    {
        var query = (await _businessRepository.WithDetailsAsync(x => x.Specs, x => x.Templates, x => x.Groups))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter),
                x => x.BusinessName.Contains(input.Filter!) || x.BusinessCode.Contains(input.Filter!))
            .WhereIf(input.CategoryId.HasValue, x => x.CategoryId == input.CategoryId);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderByDescending(x => x.CreationTime).Skip(input.SkipCount).Take(input.MaxResultCount));

        var categoryNames = await GetCategoryNameMapAsync(entities.Select(e => e.CategoryId));
        var dtos = entities.Select(e => MapToDto(e, categoryNames)).ToList();
        return new PagedResultDto<NotificationBusinessDto>(totalCount, dtos);
    }

    public async Task<NotificationBusinessDto> GetAsync(Guid id)
    {
        var entity = await GetWithDetailsAsync(id);
        var categoryNames = await GetCategoryNameMapAsync(new[] { entity.CategoryId });
        return MapToDto(entity, categoryNames);
    }

    public async Task<NotificationBusinessDto> CreateAsync(CreateNotificationBusinessDto input)
    {
        var suffix = (input.CodeSuffix ?? string.Empty).Trim().ToLowerInvariant();
        if (!CodeSuffixRegex.IsMatch(suffix))
        {
            throw new UserFriendlyException("业务编码必须为 3-16 位小写字母");
        }

        if (!await (await _categoryRepository.GetQueryableAsync()).AnyAsync(c => c.Id == input.CategoryId))
        {
            throw new UserFriendlyException("所属分类不存在");
        }

        var fullCode = $"{input.CategoryId}-{suffix}";
        if (await (await _businessRepository.GetQueryableAsync()).AnyAsync(x => x.BusinessCode == fullCode))
        {
            throw new UserFriendlyException($"业务编码 {fullCode} 已存在");
        }

        var entity = new NotificationBusinessEntity(GuidGenerator.Create())
        {
            CategoryId = input.CategoryId,
            BusinessName = input.BusinessName,
            BusinessCode = fullCode,
            Description = input.Description,
            DefaultLevel = input.DefaultLevel,
            IsEnabled = input.IsEnabled
        };
        ApplyTemplates(entity, input.Templates);

        await _businessRepository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<NotificationBusinessDto> UpdateAsync(Guid id, UpdateNotificationBusinessDto input)
    {
        var entity = await GetWithDetailsAsync(id);
        entity.BusinessName = input.BusinessName;
        entity.Description = input.Description;
        entity.DefaultLevel = input.DefaultLevel;
        entity.IsEnabled = input.IsEnabled;

        entity.Templates.Clear();
        ApplyTemplates(entity, input.Templates);

        await _businessRepository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _businessRepository.DeleteAsync(id);
    }

    public async Task<List<NotificationSpecDto>> GetSpecsAsync(Guid businessId)
    {
        var specs = await AsyncExecuter.ToListAsync(
            (await _specRepository.GetQueryableAsync()).Where(s => s.BusinessId == businessId));
        return specs.Select(MapSpecToDto).OrderBy(s => s.Level).ToList();
    }

    public async Task SetSpecsAsync(Guid businessId, List<NotificationSpecDto> specs)
    {
        var existing = await AsyncExecuter.ToListAsync(
            (await _specRepository.GetQueryableAsync()).Where(s => s.BusinessId == businessId));
        await _specRepository.DeleteManyAsync(existing);

        var newSpecs = specs.Select(s => new NotificationSpecEntity(GuidGenerator.Create())
        {
            BusinessId = businessId,
            Level = s.Level,
            IsEnabled = s.IsEnabled,
            Channels = ChannelsToCsv(s.Channels),
            ConsecutivePeriods = s.ConsecutivePeriods,
            PeriodMinutes = s.PeriodMinutes,
            Aggregation = s.Aggregation,
            Comparison = s.Comparison,
            Threshold = s.Threshold
        }).ToList();
        await _specRepository.InsertManyAsync(newSpecs, autoSave: true);
    }

    public async Task<List<long>> GetGroupIdsAsync(Guid businessId)
    {
        return await AsyncExecuter.ToListAsync(
            (await _groupBindingRepository.GetQueryableAsync()).Where(g => g.BusinessId == businessId).Select(g => g.GroupId));
    }

    public async Task SetGroupsAsync(Guid businessId, List<long> groupIds)
    {
        var existing = await AsyncExecuter.ToListAsync(
            (await _groupBindingRepository.GetQueryableAsync()).Where(g => g.BusinessId == businessId));
        await _groupBindingRepository.DeleteManyAsync(existing);

        var bindings = groupIds.Distinct().Select(gid => new NotificationBusinessGroupEntity(GuidGenerator.Create())
        {
            BusinessId = businessId,
            GroupId = gid
        }).ToList();
        await _groupBindingRepository.InsertManyAsync(bindings, autoSave: true);
    }

    private async Task<NotificationBusinessEntity> GetWithDetailsAsync(Guid id)
    {
        var entity = await (await _businessRepository.WithDetailsAsync(x => x.Specs, x => x.Templates, x => x.Groups))
            .FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(NotificationBusinessEntity), id);
        }
        return entity;
    }

    private void ApplyTemplates(NotificationBusinessEntity entity, List<NotificationTemplateDto> templates)
    {
        foreach (var tpl in templates)
        {
            entity.Templates.Add(new NotificationTemplateEntity(GuidGenerator.Create())
            {
                BusinessId = entity.Id,
                ChannelType = tpl.ChannelType,
                Title = tpl.Title,
                Content = tpl.Content,
                IsEnabled = tpl.IsEnabled
            });
        }
    }

    private async Task<Dictionary<long, string>> GetCategoryNameMapAsync(IEnumerable<long> categoryIds)
    {
        var ids = categoryIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<long, string>();
        }
        var categories = await AsyncExecuter.ToListAsync(
            (await _categoryRepository.GetQueryableAsync()).Where(c => ids.Contains(c.Id)));
        return categories.ToDictionary(c => c.Id, c => c.Name);
    }

    private static NotificationBusinessDto MapToDto(NotificationBusinessEntity entity, Dictionary<long, string> categoryNames)
    {
        var channels = entity.Specs
            .Where(s => s.IsEnabled)
            .SelectMany(s => ChannelsFromCsv(s.Channels))
            .Distinct()
            .ToList();

        return new NotificationBusinessDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = categoryNames.TryGetValue(entity.CategoryId, out var name) ? name : null,
            BusinessName = entity.BusinessName,
            BusinessCode = entity.BusinessCode,
            Description = entity.Description,
            DefaultLevel = entity.DefaultLevel,
            IsEnabled = entity.IsEnabled,
            CreationTime = entity.CreationTime,
            Templates = entity.Templates.Select(t => new NotificationTemplateDto
            {
                Id = t.Id,
                ChannelType = t.ChannelType,
                Title = t.Title,
                Content = t.Content,
                IsEnabled = t.IsEnabled
            }).ToList(),
            ConfiguredChannels = channels,
            GroupCount = entity.Groups.Count
        };
    }

    private static NotificationSpecDto MapSpecToDto(NotificationSpecEntity s) => new()
    {
        Id = s.Id,
        Level = s.Level,
        IsEnabled = s.IsEnabled,
        Channels = ChannelsFromCsv(s.Channels),
        ConsecutivePeriods = s.ConsecutivePeriods,
        PeriodMinutes = s.PeriodMinutes,
        Aggregation = s.Aggregation,
        Comparison = s.Comparison,
        Threshold = s.Threshold
    };

    private static string? ChannelsToCsv(List<NotificationChannelType> channels)
        => channels.Count == 0 ? null : string.Join(",", channels.Select(c => (int)c));

    private static List<NotificationChannelType> ChannelsFromCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return new List<NotificationChannelType>();
        }
        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => (NotificationChannelType)int.Parse(s.Trim()))
            .ToList();
    }
}
