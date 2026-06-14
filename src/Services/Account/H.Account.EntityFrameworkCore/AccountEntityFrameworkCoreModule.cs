using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace H.Account.EntityFrameworkCore;

[DependsOn(
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule)
)]
public class AccountEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("AccountDb");

        context.Services.AddAbpDbContext<AccountDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.UseSqlServer();
        });
        
        // 配置默认数据库连接字符串
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings["AbpSettingManagement"] = connectionString;
        });
    }
}
