using H.Education.Application.Contracts;
using H.Education.Application.Textbook;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 课时资源应用服务：登记五类资源并补录平台直链
/// </summary>
public class TeachingResourceAppService : ApplicationService, ITeachingResourceAppService
{
    private readonly IRepository<TeachingResourceEntity, Guid> _resourceRepository;
    private readonly IRepository<TextbookEntity, Guid> _textbookRepository;

    public TeachingResourceAppService(
        IRepository<TeachingResourceEntity, Guid> resourceRepository,
        IRepository<TextbookEntity, Guid> textbookRepository)
    {
        _resourceRepository = resourceRepository;
        _textbookRepository = textbookRepository;
    }

    public async Task<BaseOutput<TeachingResourceDto>> CreateAsync(CreateTeachingResourceDto input)
    {
        var textbook = await GetTextbookAsync(input.TextbookId);

        var entity = new TeachingResourceEntity
        {
            TextbookId = textbook.Id,
            Unit = input.Unit.Trim(),
            UnitSort = await ResolveUnitSortAsync(textbook.Id, input.Unit, input.UnitSort),
            Lesson = NormalizeOptional(input.Lesson),
            Type = (int)input.Type,
            Title = input.Title.Trim(),
            Url = TextbookLinks.NormalizeUrl(input.Url, "资源链接"),
            SourceName = NormalizeOptional(input.SourceName) ?? $"{TextbookChannels.PlatformName}·课程教学",
            Remark = NormalizeOptional(input.Remark)
        };

        entity = await _resourceRepository.InsertAsync(entity);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput<TeachingResourceDto>> UpdateAsync(Guid id, UpdateTeachingResourceDto input)
    {
        var textbook = await GetTextbookAsync(input.TextbookId);
        var entity = await _resourceRepository.GetAsync(id);

        entity.TextbookId = textbook.Id;
        entity.Unit = input.Unit.Trim();
        entity.UnitSort = input.UnitSort > 0
            ? input.UnitSort
            : await ResolveUnitSortAsync(textbook.Id, entity.Unit, 0);
        entity.Lesson = NormalizeOptional(input.Lesson);
        entity.Type = (int)input.Type;
        entity.Title = input.Title.Trim();
        entity.Url = TextbookLinks.NormalizeUrl(input.Url, "资源链接");
        entity.SourceName = NormalizeOptional(input.SourceName);
        entity.Remark = NormalizeOptional(input.Remark);

        await _resourceRepository.UpdateAsync(entity);
        return new(entity.ToDto());
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        await _resourceRepository.DeleteAsync(id);
        return new();
    }

    private async Task<TextbookEntity> GetTextbookAsync(Guid textbookId)
    {
        var textbook = await _textbookRepository.FindAsync(textbookId);
        return textbook ?? throw new UserFriendlyException("所属教材不存在，可能已被删除，请刷新教材列表");
    }

    /// <summary>
    /// 单元序号：同名单元沿用已有序号，新单元排到末尾，保证详情页按单元分组时顺序稳定
    /// </summary>
    private async Task<int> ResolveUnitSortAsync(Guid textbookId, string unit, int unitSort)
    {
        if (unitSort > 0) return unitSort;

        var query = await _resourceRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            query.Where(x => x.TextbookId == textbookId).Select(x => new { x.Unit, x.UnitSort }));

        var sameUnit = rows.FirstOrDefault(x => x.Unit == unit);
        return sameUnit?.UnitSort ?? (rows.Count == 0 ? 1 : rows.Max(x => x.UnitSort) + 1);
    }

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
