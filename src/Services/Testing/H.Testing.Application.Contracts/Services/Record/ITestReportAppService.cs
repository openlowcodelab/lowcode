using H.Abp.Application.Contracts;
using H.Util.Base;

namespace H.Testing.Application.Contracts;

/// <summary>
/// 测试报告服务接口
/// </summary>
public interface ITestReportAppService : IAppService
{
    /// <summary>
    /// 获取项目的测试报告（聚合统计：总览、每日趋势、失败用例榜、环境统计）
    /// </summary>
    Task<BaseOutput<TestReportDto>> GetReportAsync(long projectId, int days = 14);
}
