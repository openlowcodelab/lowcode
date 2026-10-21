using H.Abp.Application.Contracts;
using H.Education.Application.Contracts;
using H.Education.Application.Textbook;
using H.Education.EntityFrameworkCore;
using H.Util.Base;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace H.Education.Application.Services;

/// <summary>
/// 教材应用服务：教材目录维护、公开资源入口登记，以及把内置目录展开成五类课时资源
/// </summary>
public class TextbookAppService : ApplicationService, ITextbookAppService
{
    private readonly IRepository<TextbookEntity, Guid> _textbookRepository;
    private readonly IRepository<TeachingResourceEntity, Guid> _resourceRepository;

    public TextbookAppService(
        IRepository<TextbookEntity, Guid> textbookRepository,
        IRepository<TeachingResourceEntity, Guid> resourceRepository)
    {
        _textbookRepository = textbookRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<BaseOutput<PagedResultDto<TextbookDto>>> GetListAsync(TextbookQueryDto input)
    {
        var query = await _textbookRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim();
            query = query.Where(x => x.Name.Contains(filter)
                || x.Version.Contains(filter)
                || x.Grade.Contains(filter)
                || (x.Publisher != null && x.Publisher.Contains(filter)));
        }
        if (input.Stage.HasValue)
        {
            var stage = (int)input.Stage.Value;
            query = query.Where(x => x.Stage == stage);
        }
        if (input.Subject.HasValue)
        {
            var subject = (int)input.Subject.Value;
            query = query.Where(x => x.Subject == subject);
        }
        if (!string.IsNullOrWhiteSpace(input.Version))
        {
            var version = input.Version.Trim();
            query = query.Where(x => x.Version == version);
        }
        if (!string.IsNullOrWhiteSpace(input.Grade))
        {
            var grade = input.Grade.Trim();
            query = query.Where(x => x.Grade == grade);
        }
        if (input.Volume.HasValue)
        {
            var volume = (int)input.Volume.Value;
            query = query.Where(x => x.Volume == volume);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);
        var maxResult = input.MaxResultCount > 0 ? input.MaxResultCount : 20;
        // Sort 存的是年级序号（一年级 1…九年级 9、高一 10），直接按它排才是"一→九→高"的读法，字典序会把"九年级"排到"一年级"前
        var entities = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Sort).ThenBy(x => x.Stage).ThenBy(x => x.Subject).ThenBy(x => x.Volume)
                .Skip(input.SkipCount).Take(maxResult));

        var dtos = entities.Select(x => x.ToDto()).ToList();
        await FillResourceCountsAsync(dtos);

        return new(new PagedResultDto<TextbookDto>(totalCount, dtos));
    }

    public async Task<BaseOutput<TextbookStatisticsDto>> GetStatisticsAsync()
    {
        var textbookQuery = await _textbookRepository.GetQueryableAsync();
        var resourceQuery = await _resourceRepository.GetQueryableAsync();

        var statistics = new TextbookStatisticsDto
        {
            TextbookCount = await AsyncExecuter.CountAsync(textbookQuery),
            ResourceCount = await AsyncExecuter.CountAsync(resourceQuery)
        };

        var typeRows = await AsyncExecuter.ToListAsync(
            resourceQuery
                .GroupBy(x => x.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    Linked = g.Count(x => x.Url != null && x.Url != "")
                }));

        // 教材册数按学科统计，资源条数按学科统计后要并入同一张表，故分开查再在内存里合并
        var bookRows = await AsyncExecuter.ToListAsync(
            textbookQuery.GroupBy(x => x.Subject).Select(g => new { Subject = g.Key, Count = g.Count() }));

        var subjectRows = await AsyncExecuter.ToListAsync(
            resourceQuery
                .Join(textbookQuery, resource => resource.TextbookId, textbook => textbook.Id,
                    (resource, textbook) => new { textbook.Subject, resource.Url })
                .GroupBy(x => x.Subject)
                .Select(g => new
                {
                    Subject = g.Key,
                    Count = g.Count(),
                    Linked = g.Count(x => x.Url != null && x.Url != "")
                }));

        var subjects = bookRows.Select(x => x.Subject)
            .Concat(subjectRows.Select(x => x.Subject))
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        foreach (var subject in subjects)
        {
            var resource = subjectRows.FirstOrDefault(x => x.Subject == subject);
            statistics.BySubject.Add(new TextbookSubjectStatDto
            {
                Subject = (TextbookSubjectEnum)subject,
                TextbookCount = bookRows.FirstOrDefault(x => x.Subject == subject)?.Count ?? 0,
                ResourceCount = resource?.Count ?? 0,
                LinkedResourceCount = resource?.Linked ?? 0
            });
        }

        foreach (var type in EducationEnumExtensions.AllResourceTypes)
        {
            var row = typeRows.FirstOrDefault(x => x.Type == (int)type);
            statistics.ByType.Add(new TeachingResourceTypeStatDto
            {
                Type = type,
                Count = row?.Count ?? 0,
                LinkedCount = row?.Linked ?? 0
            });
        }

        statistics.MissingUrlCount = statistics.ResourceCount - typeRows.Sum(x => x.Linked);
        return new(statistics);
    }

    public async Task<BaseOutput<TextbookFilterOptionsDto>> GetFilterOptionsAsync()
    {
        var query = await _textbookRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            query.Select(x => new { x.Version, x.Grade, x.Publisher }));

        return new(new TextbookFilterOptionsDto
        {
            Versions = rows.Select(x => x.Version).Distinct().OrderBy(x => x).ToList(),
            // 年级是中文写法，按 "一/二/…/九/高一" 的实际顺序排，字典序会把"九年级"排到"一年级"后面
            Grades = rows.Select(x => x.Grade).Distinct()
                .OrderBy(EducationGrades.Parse)
                .ThenBy(x => x)
                .ToList(),
            Publishers = rows.Select(x => x.Publisher)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct()
                .OrderBy(x => x)
                .ToList()
        });
    }

    public async Task<BaseOutput<TextbookDto>> GetAsync(Guid id)
    {
        var textbook = await _textbookRepository.GetAsync(id);
        var dto = textbook.ToDto();
        await FillResourceCountsAsync([dto]);
        return new(dto);
    }

    public async Task<BaseOutput<TextbookDetailDto>> GetDetailAsync(Guid id)
    {
        var textbook = await _textbookRepository.GetAsync(id);
        var query = await _resourceRepository.GetQueryableAsync();
        var resources = await AsyncExecuter.ToListAsync(
            query.Where(x => x.TextbookId == id)
                .OrderBy(x => x.UnitSort).ThenBy(x => x.Type).ThenBy(x => x.Title));

        var dto = textbook.ToDetailDto();
        dto.Resources = resources.Select(x => x.ToDto()).ToList();
        dto.LocatePath = BuildLocatePath(textbook);
        await FillResourceCountsAsync([dto]);
        return new(dto);
    }

    public async Task<BaseOutput<TextbookDto>> CreateAsync(CreateTextbookDto input)
    {
        var entity = new TextbookEntity
        {
            Stage = (int)input.Stage,
            Subject = (int)input.Subject,
            Version = input.Version.Trim(),
            Grade = input.Grade.Trim(),
            Volume = (int)input.Volume,
            Publisher = NormalizeOptional(input.Publisher),
            UnitCount = input.UnitCount,
            UnitLabel = NormalizeOptional(input.UnitLabel) ?? "第{0}单元",
            BookUrl = TextbookLinks.NormalizeUrl(input.BookUrl, "电子课本地址"),
            CourseUrl = TextbookLinks.NormalizeUrl(input.CourseUrl, "课程教学地址"),
            Source = (int)TextbookSourceEnum.Manual,
            Sort = EducationGrades.Parse(input.Grade),
            Remark = NormalizeOptional(input.Remark)
        };
        entity.Name = string.IsNullOrWhiteSpace(input.Name) ? BuildName(entity) : input.Name.Trim();

        await EnsureNotDuplicatedAsync(entity, Guid.Empty);

        entity = await _textbookRepository.InsertAsync(entity, autoSave: true);
        await SyncUnitResourcesAsync(entity);

        var dto = entity.ToDto();
        await FillResourceCountsAsync([dto]);
        return new(dto);
    }

    public async Task<BaseOutput<TextbookDto>> UpdateAsync(Guid id, UpdateTextbookDto input)
    {
        var entity = await _textbookRepository.GetAsync(id);

        entity.Stage = (int)input.Stage;
        entity.Subject = (int)input.Subject;
        entity.Version = input.Version.Trim();
        entity.Grade = input.Grade.Trim();
        entity.Volume = (int)input.Volume;
        entity.Publisher = NormalizeOptional(input.Publisher);
        entity.UnitCount = input.UnitCount;
        entity.UnitLabel = NormalizeOptional(input.UnitLabel) ?? "第{0}单元";
        entity.BookUrl = TextbookLinks.NormalizeUrl(input.BookUrl, "电子课本地址");
        entity.CourseUrl = TextbookLinks.NormalizeUrl(input.CourseUrl, "课程教学地址");
        entity.Sort = EducationGrades.Parse(input.Grade);
        entity.Remark = NormalizeOptional(input.Remark);
        entity.Name = string.IsNullOrWhiteSpace(input.Name) ? BuildName(entity) : input.Name.Trim();

        await EnsureNotDuplicatedAsync(entity, id);

        await _textbookRepository.UpdateAsync(entity, autoSave: true);
        await SyncUnitResourcesAsync(entity);

        var dto = entity.ToDto();
        await FillResourceCountsAsync([dto]);
        return new(dto);
    }

    public async Task<BaseOutput> DeleteAsync(Guid id)
    {
        var entity = await _textbookRepository.GetAsync(id);

        await _resourceRepository.DeleteAsync(x => x.TextbookId == id, autoSave: true);
        await _textbookRepository.DeleteAsync(entity);

        return new();
    }

    public async Task<BaseOutput<TextbookImportResultDto>> ImportBuiltInAsync()
    {
        var query = await _textbookRepository.GetQueryableAsync();
        var existing = await AsyncExecuter.ToListAsync(
            query.Select(x => new { x.Id, x.Subject, x.Version, x.Grade, x.Volume, x.BookUrl }));

        var result = new TextbookImportResultDto();
        foreach (var seed in TextbookCatalogData.All)
        {
            var ebookUrl = seed.EbookPath is null ? null : TextbookLinks.EbookUrl(seed.EbookPath);
            var hit = existing.FirstOrDefault(x => x.Subject == (int)seed.Subject
                && x.Version == seed.Version
                && x.Grade == seed.Grade
                && x.Volume == (int)seed.Volume);
            if (hit is not null)
            {
                result.SkippedTextbookCount++;

                // 早先导入的册次只记了平台频道入口，这里把电子课本直链补上，其余字段保持不动
                if (ebookUrl is not null
                    && (string.IsNullOrWhiteSpace(hit.BookUrl) || hit.BookUrl == TextbookChannels.MaterialUrl))
                {
                    var stored = await _textbookRepository.GetAsync(hit.Id);
                    stored.BookUrl = ebookUrl;
                    await _textbookRepository.UpdateAsync(stored, autoSave: true);
                    result.UpdatedLinkCount++;
                }

                continue;
            }

            var entity = new TextbookEntity
            {
                Name = seed.Name,
                Stage = (int)seed.Stage,
                Subject = (int)seed.Subject,
                Version = seed.Version,
                Grade = seed.Grade,
                Volume = (int)seed.Volume,
                Publisher = seed.Publisher,
                UnitCount = seed.UnitCount,
                UnitLabel = seed.UnitLabel,
                BookUrl = ebookUrl ?? TextbookChannels.MaterialUrl,
                CourseUrl = TextbookChannels.CourseUrl,
                Source = (int)TextbookSourceEnum.BuiltIn,
                Sort = EducationGrades.Parse(seed.Grade),
                Remark = seed.Remark ?? DefaultCatalogRemark
            };

            entity = await _textbookRepository.InsertAsync(entity, autoSave: true);
            result.InsertedTextbookCount++;
            result.InsertedResourceCount += await SyncUnitResourcesAsync(entity);
        }

        var message = result.InsertedTextbookCount == 0
            ? $"{result.SkippedTextbookCount} 册教材已在目录中，未重复导入"
            : $"新增 {result.InsertedTextbookCount} 册教材、{result.InsertedResourceCount} 条课时资源，跳过 {result.SkippedTextbookCount} 册";

        result.Message = result.UpdatedLinkCount > 0
            ? $"{message}；补录 {result.UpdatedLinkCount} 册电子课本直链"
            : message;

        return new(result);
    }

    /// <summary>内置目录条目的统一备注，说明资源只到单元级、直链需人工核对</summary>
    private const string DefaultCatalogRemark = "目录取自公开教材信息，只到单元级；课时名称与资源直链需在平台内核对后补录";

    /// <summary>
    /// 按教材的单元数量补齐课时资源：只追加缺失的单元，不改动已补录的条目
    /// </summary>
    private async Task<int> SyncUnitResourcesAsync(TextbookEntity textbook)
    {
        if (textbook.UnitCount <= 0) return 0;

        var query = await _resourceRepository.GetQueryableAsync();
        var unitSorts = await AsyncExecuter.ToListAsync(
            query.Where(x => x.TextbookId == textbook.Id).Select(x => x.UnitSort));
        var maxUnit = unitSorts.Count == 0 ? 0 : unitSorts.Max();

        if (maxUnit >= textbook.UnitCount) return 0;

        var resources = new List<TeachingResourceEntity>();
        for (var index = maxUnit + 1; index <= textbook.UnitCount; index++)
        {
            var unit = TextbookCatalogData.FormatUnit(textbook.UnitLabel, index);
            foreach (var type in EducationEnumExtensions.AllResourceTypes)
            {
                resources.Add(new TeachingResourceEntity
                {
                    TextbookId = textbook.Id,
                    Unit = unit,
                    UnitSort = index,
                    Type = (int)type,
                    Title = $"{unit} {type.GetResourceTypeName()}",
                    SourceName = $"{TextbookChannels.PlatformName}·课程教学"
                });
            }
        }

        await _resourceRepository.InsertManyAsync(resources, autoSave: true);
        return resources.Count;
    }

    /// <summary>
    /// 填充资源条数、视频课程条数与已补录直链条数
    /// </summary>
    private async Task FillResourceCountsAsync(List<TextbookDto> dtos)
    {
        if (dtos.Count == 0) return;

        var ids = dtos.Select(x => x.Id).ToList();
        var videoType = (int)TeachingResourceTypeEnum.VideoCourse;

        var query = await _resourceRepository.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            query.Where(x => ids.Contains(x.TextbookId))
                .GroupBy(x => x.TextbookId)
                .Select(g => new
                {
                    TextbookId = g.Key,
                    Total = g.Count(),
                    Video = g.Count(x => x.Type == videoType),
                    Linked = g.Count(x => x.Url != null && x.Url != "")
                }));

        var map = rows.ToDictionary(x => x.TextbookId, x => x);
        foreach (var dto in dtos)
        {
            if (!map.TryGetValue(dto.Id, out var row)) continue;
            dto.ResourceCount = row.Total;
            dto.VideoCourseCount = row.Video;
            dto.LinkedResourceCount = row.Linked;
        }
    }

    private async Task EnsureNotDuplicatedAsync(TextbookEntity textbook, Guid selfId)
    {
        var query = await _textbookRepository.GetQueryableAsync();
        var duplicated = await AsyncExecuter.AnyAsync(
            query.Where(x => x.Id != selfId
                && x.Subject == textbook.Subject
                && x.Version == textbook.Version
                && x.Grade == textbook.Grade
                && x.Volume == textbook.Volume));
        if (duplicated)
        {
            throw new UserFriendlyException(
                $"{textbook.Version} {textbook.Grade}{SubjectName(textbook)}{VolumeName(textbook)} 已在教材目录中，可直接进详情页补录资源");
        }
    }

    private static string SubjectName(TextbookEntity textbook) => ((TextbookSubjectEnum)textbook.Subject).GetSubjectName();

    private static string VolumeName(TextbookEntity textbook) => ((TextbookVolumeEnum)textbook.Volume).GetVolumeName();

    private static string StageName(TextbookEntity textbook) => ((SchoolStageEnum)textbook.Stage).GetStageName();

    private static string BuildName(TextbookEntity textbook) =>
        $"{textbook.Grade}{SubjectName(textbook)}{VolumeName(textbook)}（{textbook.Version}）";

    private static string BuildLocatePath(TextbookEntity textbook) =>
        $"课程教学 › {StageName(textbook)} › {SubjectName(textbook)} › {textbook.Grade} › {VolumeName(textbook)}";

    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
