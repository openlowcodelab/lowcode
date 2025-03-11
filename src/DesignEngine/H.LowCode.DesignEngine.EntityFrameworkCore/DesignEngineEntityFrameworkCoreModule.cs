using H.LowCode.DesignEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.LowCode.DesignEngine.EntityFrameworkCore;

public class DesignEngineEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IFormDataRepository, FormDataRepository>();
        context.Services.AddScoped<ITableDataRepository, TableDataRepository>();

        context.Services.AddScoped(typeof(EntityTypeManager));

        context.Services.AddDbContext<DesignEngineDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            options.UseSqlServer(connectionString);
        });
    }
}