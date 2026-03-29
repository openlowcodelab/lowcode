using H.Account.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace H.Account.DbMigrator;

/// <summary>
/// DbContext 工厂类，用于 EF Core 迁移
/// </summary>
public class AccountDbContextFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    public AccountDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AccountDb;Trusted_Connection=true;");

        return new AccountDbContext(optionsBuilder.Options);
    }
}
