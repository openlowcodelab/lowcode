using H.LowCode.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            .GetRequiredService<DbMigratorDbContext>()
            .Database
            .MigrateAsync();
    }
}
