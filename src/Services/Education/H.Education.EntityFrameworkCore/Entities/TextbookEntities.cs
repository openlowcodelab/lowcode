using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace H.Education.EntityFrameworkCore;

/// <summary>
/// 教材实体，一行对应一册教材（如 "统编版 三年级 语文 上册"）
/// </summary>
public class TextbookEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>教材名称，如 "三年级语文上册（统编版）"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>学段（1 小学、2 初中、3 高中）</summary>
    public int Stage { get; set; }

    /// <summary>学科（对应 Contracts 的 TextbookSubjectEnum）</summary>
    public int Subject { get; set; }

    /// <summary>教材版本，如 "统编版"、"人教版"、"外研版"</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>年级，如 "三年级"、"七年级"、"高一"</summary>
    public string Grade { get; set; } = string.Empty;

    /// <summary>册次（1 上册、2 下册、3 全一册）</summary>
    public int Volume { get; set; }

    /// <summary>出版社</summary>
    public string? Publisher { get; set; }

    /// <summary>单元（章、Module）数量</summary>
    public int UnitCount { get; set; }

    /// <summary>单元名称样式，如 "第{0}单元"、"第{0}章"、"Unit {0}"</summary>
    public string? UnitLabel { get; set; }

    /// <summary>电子课本页面地址</summary>
    public string? BookUrl { get; set; }

    /// <summary>课程教学频道地址</summary>
    public string? CourseUrl { get; set; }

    /// <summary>来源（0 内置公开目录、1 手工录入）</summary>
    public int Source { get; set; }

    /// <summary>目录排序号，学段与学科内部按它排列</summary>
    public int Sort { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}

/// <summary>
/// 课时资源实体，一行对应某一单元（或课时）下的一类资源
/// </summary>
public class TeachingResourceEntity : AuditedEntity<Guid>, IMultiTenant
{
    /// <summary>租户ID</summary>
    public virtual Guid? TenantId { get; set; }

    /// <summary>所属教材</summary>
    public Guid TextbookId { get; set; }

    /// <summary>所属教材导航属性</summary>
    public virtual TextbookEntity? Textbook { get; set; }

    /// <summary>单元名称，如 "第三单元"、"Unit 2"</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>单元排序号</summary>
    public int UnitSort { get; set; }

    /// <summary>课时名称，内置目录只到单元级，留空表示待补录</summary>
    public string? Lesson { get; set; }

    /// <summary>资源类型（1 视频课程、2 课件、3 教学设计、4 学习任务单、5 课后练习）</summary>
    public int Type { get; set; }

    /// <summary>资源标题</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>资源直链，留空表示只登记了平台入口</summary>
    public string? Url { get; set; }

    /// <summary>来源名称，如 "国家中小学智慧教育平台·课程教学"</summary>
    public string? SourceName { get; set; }

    /// <summary>备注</summary>
    public string? Remark { get; set; }
}
