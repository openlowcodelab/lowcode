using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Services;

namespace H.AutoTest.Application;

/// <summary>
/// AutoTest 应用服务基类，提供租户感知的数据路径
/// </summary>
public abstract class AutoTestAppServiceBase : ApplicationService
{
    protected readonly string _baseDataPath;

    protected AutoTestAppServiceBase(IConfiguration configuration)
    {
        _baseDataPath = configuration["DataPath"] ?? "data";
    }

    /// <summary>
    /// 获取当前租户的数据目录路径（data/{tenantId}/ 或 data/default/）
    /// </summary>
    protected string GetTenantDataPath()
    {
        var tenantId = CurrentTenant?.Id;
        var tenantDir = tenantId?.ToString() ?? "default";
        var path = Path.Combine(_baseDataPath, tenantDir);
        return path;
    }
}
