using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.Enterprise.EntityFrameworkCore;

public class EnterpriseEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("EnterpriseDb");

        context.Services.AddDbContext<EnterpriseDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        // 自动创建数据库（如果不存在）
        using var scope = context.ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EnterpriseDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
