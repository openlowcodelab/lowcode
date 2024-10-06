using H.LowCode.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

internal class LowCodeDbContext : DbContext
{
    //private readonly ConnectionInfo _connectionInfo;

    //public LowCodeDbContext(ConnectionInfo connectionInfo)
    //{
    //    _connectionInfo = connectionInfo;
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ////设置连接字符串
        //optionsBuilder.UseSqlServer(_connectionInfo.ConnectionString);

        ////注册EFcore拦截器
        ////optionsBuilder.AddInterceptors(new QueryWithNoLockDbCommandInterceptor());

        ////表重复注册
        //optionsBuilder.ReplaceService<IProviderConventionSetBuilder, CustomizeRelationalConventionSetBuilder>();
        //optionsBuilder.ReplaceService<IModelValidator, CustomizeRelationalModelValidator>();
        ////Linq查询扩展
        //optionsBuilder.ReplaceService<IRelationalSqlTranslatingExpressionVisitorFactory,
        //    CustomizeSqlTranslatingExpressionVisitorFactory>();

        ////Entity Attributes填充
        //optionsBuilder.ReplaceService<IInternalEntityEntryFactory, CustomizeEntityEntryFactory>();
        ////实体中不可空类型支持
        //optionsBuilder.ReplaceService<IShapedQueryCompilingExpressionVisitorFactory,
        //    CustomizeRelationalShapedQueryCompilingExpressionVisitorFactory>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entities = new LowCodeEntityProvider().GetAllEntitySchemas();
        foreach (var entity in entities)
        {
            ////表映射
            //var entityBuilder = modelBuilder.Entity(entityProxyType).ToTable(entityMetadata.Name);
            ////简单属性
            //RegisterSimpleProperties(entityBuilder, entityMetadata, entityType);
            ////主键
            //entityBuilder.HasKey(entityMetadata.PrimaryKeyName);
            ////逻辑删除过滤
            //if (entityMetadata.EnableSoftDelete)
            //{
            //    modelBuilder.Entity(entityProxyType, entityBuilder =>
            //    {
            //        entityBuilder.HasQueryFilter(ApplyEntityFilterTo(entityProxyType));
            //    });
            //}
        }
    }
}