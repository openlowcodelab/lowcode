using H.Notification.Application.Contracts;
using H.Notification.EntityFrameworkCore;
using H.Util.Base;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Notification.Application;

public class NotificationCategoryAppService : ApplicationService, INotificationCategoryAppService
{
    private readonly IRepository<NotificationCategory, long> _categoryRepository;
    private readonly IRepository<NotificationBusinessEntity, Guid> _businessRepository;

    public NotificationCategoryAppService(
        IRepository<NotificationCategory, long> categoryRepository,
        IRepository<NotificationBusinessEntity, Guid> businessRepository)
    {
        _categoryRepository = categoryRepository;
        _businessRepository = businessRepository;
    }

    public async Task<BaseOutput<List<NotificationCategoryDto>>> GetAllAsync()
    {
        var categories = await AsyncExecuter.ToListAsync(
            (await _categoryRepository.GetQueryableAsync()).OrderBy(x => x.Sort).ThenBy(x => x.Id));

        var businessQuery = await _businessRepository.GetQueryableAsync();
        var counts = await AsyncExecuter.ToListAsync(
            businessQuery.GroupBy(x => x.CategoryId).Select(g => new { CategoryId = g.Key, Count = g.Count() }));
        var countMap = counts.ToDictionary(x => x.CategoryId, x => x.Count);

        return new(categories.Select(c => new NotificationCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Sort = c.Sort,
            IsEnabled = c.IsEnabled,
            CreationTime = c.CreationTime,
            BusinessCount = countMap.TryGetValue(c.Id, out var n) ? n : 0
        }).ToList());
    }

    public async Task<BaseOutput<NotificationCategoryDto>> GetAsync(long id)
    {
        var entity = await _categoryRepository.GetAsync(id);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<NotificationCategoryDto>> CreateAsync(CreateNotificationCategoryDto input)
    {
        var entity = new NotificationCategory
        {
            Name = input.Name,
            Description = input.Description,
            Sort = input.Sort,
            IsEnabled = input.IsEnabled
        };
        entity = await _categoryRepository.InsertAsync(entity, autoSave: true);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput<NotificationCategoryDto>> UpdateAsync(long id, UpdateNotificationCategoryDto input)
    {
        var entity = await _categoryRepository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.Sort = input.Sort;
        entity.IsEnabled = input.IsEnabled;
        entity = await _categoryRepository.UpdateAsync(entity, autoSave: true);
        return new(MapToDto(entity));
    }

    public async Task<BaseOutput> DeleteAsync(long id)
    {
        var hasBusiness = await (await _businessRepository.GetQueryableAsync()).AnyAsync(x => x.CategoryId == id);
        if (hasBusiness)
        {
            throw new UserFriendlyException("该分类下存在通知业务，无法删除");
        }
        await _categoryRepository.DeleteAsync(id);
        return new();
    }

    private static NotificationCategoryDto MapToDto(NotificationCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Sort = entity.Sort,
        IsEnabled = entity.IsEnabled,
        CreationTime = entity.CreationTime
    };
}
