using H.Organization.Application.Contracts;
using H.Organization.Application.Services;
using H.Organization.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Organization.Application;

[DependsOn(
    typeof(OrganizationApplicationContractsModule),
    typeof(OrganizationEntityFrameworkCoreModule)
)]
public class OrganizationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 注册应用服务
        context.Services.AddScoped<IOrganizationService, OrganizationService>();
        context.Services.AddScoped<IMemberService, MemberService>();
        context.Services.AddScoped<IRoleService, RoleService>();
    }
}
