using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Education.Application.Contracts;

/// <summary>
/// 教材应用服务：维护中小学教材目录（学段/学科/版本/年级/册次），并可一键导入内置的公开教材目录。
/// ABP 约定控制器生成的端点（前端由 IAppService 动态代理调用）：
///  - GET    /api/app/textbook                 分页列表（含资源条数与已补录直链数）
///  - GET    /api/app/textbook/statistics      按学科、按资源类型统计
///  - GET    /api/app/textbook/filter-options  版本/年级/出版社候选值
///  - GET    /api/app/textbook/{id}            教材基本信息
///  - GET    /api/app/textbook/{id}/detail     教材详情（含全部课时资源与平台定位路径）
///  - POST   /api/app/textbook                 新增教材
///  - PUT    /api/app/textbook/{id}            修改教材
///  - DELETE /api/app/textbook/{id}            删除教材及其课时资源
///  - POST   /api/app/textbook/import-built-in 导入内置公开教材目录（已存在的册次自动跳过）
/// </summary>
public interface ITextbookAppService : IAppService
{
    /// <summary>分页查询教材</summary>
    Task<BaseOutput<PagedResultDto<TextbookDto>>> GetListAsync(TextbookQueryDto input);

    /// <summary>教材与课时资源总览统计</summary>
    Task<BaseOutput<TextbookStatisticsDto>> GetStatisticsAsync();

    /// <summary>列表页筛选下拉的候选值</summary>
    Task<BaseOutput<TextbookFilterOptionsDto>> GetFilterOptionsAsync();

    /// <summary>获取教材基本信息</summary>
    Task<BaseOutput<TextbookDto>> GetAsync(Guid id);

    /// <summary>获取教材详情，含按单元与类型排序的课时资源</summary>
    Task<BaseOutput<TextbookDetailDto>> GetDetailAsync(Guid id);

    /// <summary>新增教材</summary>
    Task<BaseOutput<TextbookDto>> CreateAsync(CreateTextbookDto input);

    /// <summary>修改教材</summary>
    Task<BaseOutput<TextbookDto>> UpdateAsync(Guid id, UpdateTextbookDto input);

    /// <summary>删除教材，级联删除其课时资源</summary>
    Task<BaseOutput> DeleteAsync(Guid id);

    /// <summary>导入内置的公开教材目录，按 学科+版本+年级+册次 去重，可重复点击</summary>
    Task<BaseOutput<TextbookImportResultDto>> ImportBuiltInAsync();
}
