using H.LowCode.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace H.LowCode.EntityFrameworkCore;

public class LowCodeEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddDbContext<LowCodeDbContext>(options =>
        {
            var connectionString = context.Services.GetConfiguration().GetConnectionString("Default");
            options.UseSqlServer(connectionString);
        });

        context.Services.AddScoped(typeof(EntityTypeManager));
    }
}