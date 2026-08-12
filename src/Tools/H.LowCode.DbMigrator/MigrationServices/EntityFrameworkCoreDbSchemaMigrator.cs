using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace H.LowCode.DbMigrator;

public class EntityFrameworkCoreDbSchemaMigrator : IDbSchemaMigrator
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        await _serviceProvider
            .GetRequiredService<MigratorDbContext>()
            .Database
            .MigrateAsync();
    }
}
