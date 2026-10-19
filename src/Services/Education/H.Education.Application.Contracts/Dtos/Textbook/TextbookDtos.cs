using System.ComponentModel.DataAnnotations;
using H.Abp.Application.Contracts;

namespace H.Education.Application.Contracts;

/// <summary>
/// 公开资源来源频道常量：内置目录只登记平台入口，具体课时直链由使用者在详情页补录
/// </summary>
public static class TextbookChannels
{
    /// <summary>平台名称，作为资源条目的来源标注</summary>
    public const string PlatformName = "国家中小学智慧教育平台";

    /// <summary>课程教学频道（视频课程、课件、教学设计、学习任务单、课后练习）</summary>
    public const string CourseUrl = "https://basic.smartedu.cn/syncClassroom";

    /// <summary>教材频道（电子课本）</summary>
    public const string MaterialUrl = "https://basic.smartedu.cn/tchMaterial";
}

/// <summary>
/// 教材 DTO
/// </summary>
public class TextbookDto
{
    public Guid Id { get; set; }

    /// <summary>教材名称，如 "一年级语文上册（统编版）"</summary>
    public string Name { get; set; } = string.Empty;

    public SchoolStageEnum Stage { get; set; }

    public TextbookSubjectEnum Subject { get; set; }

    /// <summary>版本，如 "统编版"、"人教版"、"外研版"</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>年级，如 "三年级"、"七年级"、"高一下"</summary>
    public string Grade { get; set; } = string.Empty;

    public TextbookVolumeEnum Volume { get; set; }

    /// <summary>出版社</summary>
    public string? Publisher { get; set; }

    /// <summary>单元（章、Module）数量</summary>
    public int UnitCount { get; set; }

    /// <summary>单元名称样式，如 "第{0}单元"、"第{0}章"、"Unit {0}"</summary>
    public string? UnitLabel { get; set; }

    /// <summary>已登记的课时资源条数</summary>
    public int ResourceCount { get; set; }

    /// <summary>视频课程条数，用于列表页快速判断资源是否补齐</summary>
    public int VideoCourseCount { get; set; }

    /// <summary>直链已补录的资源条数</summary>
    public int LinkedResourceCount { get; set; }

    /// <summary>电子课本页面地址</summary>
    public string? BookUrl { get; set; }

    /// <summary>课程教学频道地址</summary>
    public string? CourseUrl { get; set; }

    public TextbookSourceEnum Source { get; set; }

    public string? Remark { get; set; }

    public DateTime CreationTime { get; set; }
}

/// <summary>
/// 教材分页查询条件
/// </summary>
public class TextbookQueryDto : PagedAndSortedResultRequestDto
{
    /// <summary>教材名称、版本或年级模糊查询</summary>
    public string? Filter { get; set; }

    public SchoolStageEnum? Stage { get; set; }

    public TextbookSubjectEnum? Subject { get; set; }

    /// <summary>版本精确匹配</summary>
    public string? Version { get; set; }

    /// <summary>年级精确匹配</summary>
    public string? Grade { get; set; }

    public TextbookVolumeEnum? Volume { get; set; }
}

/// <summary>
/// 新增教材入参
/// </summary>
public class CreateTextbookDto
{
    /// <summary>留空时由 "年级 + 学科 + 册次 + 版本" 自动拼出</summary>
    [StringLength(200, ErrorMessage = "教材名称长度不能超过 200")]
    public string? Name { get; set; }

    public SchoolStageEnum Stage { get; set; }

    public TextbookSubjectEnum Subject { get; set; }

    [Required(ErrorMessage = "请填写教材版本")]
    [StringLength(32, ErrorMessage = "版本长度不能超过 32")]
    public string Version { get; set; } = string.Empty;

    [Required(ErrorMessage = "请填写年级")]
    [StringLength(32, ErrorMessage = "年级长度不能超过 32")]
    public string Grade { get; set; } = string.Empty;

    public TextbookVolumeEnum Volume { get; set; } = TextbookVolumeEnum.First;

    [StringLength(64, ErrorMessage = "出版社长度不能超过 64")]
    public string? Publisher { get; set; }

    /// <summary>单元数量，决定导入或新建时展开多少组课时资源</summary>
    [Range(0, 30, ErrorMessage = "单元数量应在 0 到 30 之间")]
    public int UnitCount { get; set; }

    /// <summary>单元名称样式，如 "第{0}单元"、"第{0}章"、"Unit {0}"</summary>
    [StringLength(32, ErrorMessage = "单元名称样式长度不能超过 32")]
    public string? UnitLabel { get; set; }

    [StringLength(500, ErrorMessage = "电子课本地址长度不能超过 500")]
    public string? BookUrl { get; set; }

    [StringLength(500, ErrorMessage = "课程教学地址长度不能超过 500")]
    public string? CourseUrl { get; set; }

    [StringLength(500, ErrorMessage = "备注长度不能超过 500")]
    public string? Remark { get; set; }
}

/// <summary>
/// 更新教材入参
/// </summary>
public class UpdateTextbookDto : CreateTextbookDto
{
}

/// <summary>
/// 课时资源 DTO
/// </summary>
public class TeachingResourceDto
{
    public Guid Id { get; set; }

    public Guid TextbookId { get; set; }

    /// <summary>单元名称，如 "第三单元"、"Unit 2"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>单元排序号</summary>
    public int UnitSort { get; set; }

    /// <summary>课时名称，如 "第 3 课 不懂就要问"；内置目录只到单元级，留空待补录</summary>
    public string? Lesson { get; set; }

    public TeachingResourceTypeEnum Type { get; set; }

    /// <summary>资源标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>资源直链；留空表示只登记了平台入口，需在平台内按定位路径查找</summary>
    public string? Url { get; set; }

    /// <summary>来源名称，如 "国家中小学智慧教育平台·课程教学"</summary>
    public string? SourceName { get; set; }

    public string? Remark { get; set; }
}

/// <summary>
/// 教材详情：教材信息 + 按单元与类型排序的全部课时资源
/// </summary>
public class TextbookDetailDto : TextbookDto
{
    /// <summary>课时资源，前端按 Unit 分组渲染</summary>
    public List<TeachingResourceDto> Resources { get; set; } = [];

    /// <summary>平台内定位路径，如 "课程教学 › 小学 › 语文 › 三年级 › 上册"，用于在平台里找到对应单元</summary>
    public string LocatePath { get; set; } = string.Empty;
}

/// <summary>
/// 新增课时资源入参
/// </summary>
public class CreateTeachingResourceDto
{
    [Required(ErrorMessage = "请指定所属教材")]
    public Guid TextbookId { get; set; }

    [Required(ErrorMessage = "请填写单元名称")]
    [StringLength(64, ErrorMessage = "单元名称长度不能超过 64")]
    public string Unit { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int UnitSort { get; set; }

    [StringLength(100, ErrorMessage = "课时名称长度不能超过 100")]
    public string? Lesson { get; set; }

    [Required(ErrorMessage = "请选择资源类型")]
    public TeachingResourceTypeEnum Type { get; set; }

    [Required(ErrorMessage = "请填写资源标题")]
    [StringLength(200, ErrorMessage = "资源标题长度不能超过 200")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "资源链接长度不能超过 1000")]
    public string? Url { get; set; }

    [StringLength(100, ErrorMessage = "来源名称长度不能超过 100")]
    public string? SourceName { get; set; }

    [StringLength(500, ErrorMessage = "备注长度不能超过 500")]
    public string? Remark { get; set; }
}

/// <summary>
/// 更新课时资源入参
/// </summary>
public class UpdateTeachingResourceDto : CreateTeachingResourceDto
{
}

/// <summary>
/// 导入内置公开目录的结果
/// </summary>
public class TextbookImportResultDto
{
    /// <summary>本次新增的教材册数</summary>
    public int InsertedTextbookCount { get; set; }

    /// <summary>本次新增的课时资源条数</summary>
    public int InsertedResourceCount { get; set; }

    /// <summary>已存在而跳过的教材册数</summary>
    public int SkippedTextbookCount { get; set; }

    /// <summary>结果说明</summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 按学科统计教材与资源数量
/// </summary>
public class TextbookSubjectStatDto
{
    public TextbookSubjectEnum Subject { get; set; }

    public int TextbookCount { get; set; }

    public int ResourceCount { get; set; }

    /// <summary>已补录直链的资源条数</summary>
    public int LinkedResourceCount { get; set; }
}

/// <summary>
/// 按类型统计课时资源数量
/// </summary>
public class TeachingResourceTypeStatDto
{
    public TeachingResourceTypeEnum Type { get; set; }

    public int Count { get; set; }

    /// <summary>已补录直链的条数</summary>
    public int LinkedCount { get; set; }
}

/// <summary>
/// 教材管理总览
/// </summary>
public class TextbookStatisticsDto
{
    public int TextbookCount { get; set; }

    public int ResourceCount { get; set; }

    /// <summary>尚未补录直链的资源条数，代表还需要人工核对的量</summary>
    public int MissingUrlCount { get; set; }

    public List<TextbookSubjectStatDto> BySubject { get; set; } = [];

    public List<TeachingResourceTypeStatDto> ByType { get; set; } = [];
}

/// <summary>
/// 列表页筛选下拉可用的候选值，取自已入库教材
/// </summary>
public class TextbookFilterOptionsDto
{
    public List<string> Versions { get; set; } = [];

    public List<string> Grades { get; set; } = [];

    public List<string> Publishers { get; set; } = [];
}
