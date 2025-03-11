using H.LowCode.RenderEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class RenderEngineEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IFormDataRepository, FormDataRepository>();
        context.Services.AddScoped<ITableDataRepository, TableDataRepository>();

        context.Services.AddScoped(typeof(EntityTypeManager));

        context.Services.AddDbContext<RenderEngineDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            options.UseSqlServer(connectionString);
        });
    }
}