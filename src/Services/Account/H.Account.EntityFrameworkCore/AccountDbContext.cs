using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;

namespace H.Account.EntityFrameworkCore;

[ConnectionStringName("AccountDb")]
public class AccountDbContext : AbpDbContext<AccountDbContext>
{
    public AccountDbContext(DbContextOptions<AccountDbContext> options) 
        : base(options)
    {
    }

    // Identity 实体
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityUserLogin> UserLogins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // 配置 Identity 实体
        modelBuilder.ConfigureIdentity();
    }
}