namespace H.LowCode.DbMigrator;

public interface IDbSchemaMigrator
{
    Task MigrateAsync();
}
