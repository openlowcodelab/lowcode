using H.LowCode.DesignEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class DesignEngineEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IFormDataRepository, FormDataRepository>();
        context.Services.AddScoped<ITableDataRepository, TableDataRepository>();

        context.Services.AddScoped(typeof(EntityTypeManager));

        // 解析连接串：优先 DesignEngineDb，回退 Default
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("DesignEngineDb")
            ?? configuration.GetConnectionString("Default");

        context.Services.AddDbContext<DesignEngineDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}