using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;

namespace H.AppLab.Host.All;

/// <summary>
/// 从认证 Cookie 中的 "TenantId" Claim 解析当前租户。
/// 企业用户登录并选择企业后，<c>EnterpriseAppService</c> 会将企业 Id 写入 "TenantId" Claim，
/// 该解析器据此设置 ABP 的当前租户（<c>ICurrentTenant</c>），从而驱动各服务的数据租户隔离。
/// </summary>
public class ClaimsTenantResolveContributor : TenantResolveContributorBase
{
    public const string ContributorName = "EnterpriseClaims";

    public override string Name => ContributorName;

    public override Task ResolveAsync(ITenantResolveContext context)
    {
        var principalAccessor = context.ServiceProvider.GetRequiredService<ICurrentPrincipalAccessor>();

        var tenantIdClaim = principalAccessor.Principal?.FindFirst("TenantId")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantIdClaim))
        {
            context.TenantIdOrName = tenantIdClaim;
        }

        return Task.CompletedTask;
    }
}
