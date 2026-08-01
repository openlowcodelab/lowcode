using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace H.File.EntityFrameworkCore;

public class FileEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("FileDb");

        context.Services.AddAbpDbContext<FileDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options => { options.UseSqlServer(); });
        Configure<AbpDbConnectionOptions>(options => { options.ConnectionStrings["FileDb"] = connectionString; });
    }
}
