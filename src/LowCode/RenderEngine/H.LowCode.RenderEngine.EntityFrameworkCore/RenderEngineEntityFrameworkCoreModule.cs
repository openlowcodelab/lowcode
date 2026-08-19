using H.LowCode.RenderEngine.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace H.LowCode.RenderEngine.EntityFrameworkCore;

public class RenderEngineEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<IFormDataRepository, FormDataRepository>();
        context.Services.AddScoped<ITableDataRepository, TableDataRepository>();

        context.Services.AddScoped(typeof(EntityTypeManager));

        // 解析连接串：优先 RenderEngineDb，回退 Default
        var configuration = context.Services.GetConfiguration();
        var connectionString = configuration.GetConnectionString("RenderEngineDb")
            ?? configuration.GetConnectionString("Default");

        // 注册 DbContext 工厂，确保每次使用都创建新的实例，避免并发访问问题
        context.Services.AddDbContextFactory<RenderEngineDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // 保持原有的 DbContext 注册方式作为备用
        context.Services.AddDbContext<RenderEngineDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
    }
}