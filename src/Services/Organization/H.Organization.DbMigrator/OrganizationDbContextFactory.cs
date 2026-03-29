using H.Organization.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace H.Organization.DbMigrator;

/// <summary>
/// DbContext 工厂类，用于 EF Core 迁移
/// </summary>
public class OrganizationDbContextFactory : IDesignTimeDbContextFactory<OrganizationDbContext>
{
    public OrganizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=OrganizationDb;Trusted_Connection=true;",
            b => b.MigrationsAssembly("H.Organization.DbMigrator")
        );

        return new OrganizationDbContext(optionsBuilder.Options);
    }
}
