using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.Account.EntityFrameworkCore;

public class AccountEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("AccountDb");

        context.Services.AddDbContext<AccountDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}
