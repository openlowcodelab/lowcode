using H.LowCode.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;

namespace H.LowCode.EntityFrameworkCore;

public class LowCodeDbContext : DbContext
{
    public string AppId { get; private set; }

    private EntityTypeManager _entityTypeManager;

    public LowCodeDbContext(DbContextOptions<LowCodeDbContext> options,
        EntityTypeManager entityTypeManager) : base(options)
    {
        _entityTypeManager = entityTypeManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ReadOnlySaveChangesInterceptor());

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

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override async void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var types = await _entityTypeManager.GetTypesAsync();
        foreach (var type in types)
        {
            //var entityBuilder1 = modelBuilder.Entity(type)
            //    .ToTable(type.Name).HasNoKey();
        }

        var dynamicEntities = await _entityTypeManager.LoadDynamicEntitiesAsync();
        for (var i = 0; i < dynamicEntities.Count(); i++)
        {
            var dynamicEntity = dynamicEntities[i];

            //表映射
            var entityBuilder = modelBuilder.Entity(dynamicEntity.EntityType)
                .ToTable(dynamicEntity.EntityName);
            //属性映射
            ConfigureProperties(entityBuilder, dynamicEntity, dynamicEntity.EntityType);
            //主键
            entityBuilder.HasKey(dynamicEntity.PrimaryKey);
            //逻辑删除过滤
            if (dynamicEntity.EnableSoftDelete)
            {
                modelBuilder.Entity(dynamicEntity.EntityType, entityBuilder =>
                {
                    entityBuilder.HasQueryFilter(SoftDeleteQueryFilterExpression(dynamicEntity.EntityType));
                });
            }
        }
    }

    private void ConfigureProperties(EntityTypeBuilder entityBuilder, DynamicEntityInfo dynamicEntity, Type entityType)
    {
        var properties = entityType.GetProperties()?.ToList();
        foreach (var field in dynamicEntity.Fields)
        {
            //不为空列
            if (!field.IsNullable)
            {
                entityBuilder.Property(field.ClrType, field.Name).IsRequired(true);
            }
            else
            {
                Type clrType = null;
                if (field.ClrType.IsValueType)
                {
                    clrType = typeof(Nullable<>).MakeGenericType(field.ClrType);
                }
                entityBuilder.Property(clrType ?? field.ClrType, field.Name);
            }

            //映射表字段
            Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder propertyBuilder = entityBuilder.Property(field.Name);
            //TODO: HasMaxLength, IsUnicode 等待配置
        }

        properties?.ForEach(p => entityBuilder.Ignore(p.Name));
    }

    /// <summary>
    /// 逻辑删除过滤
    /// </summary>
    /// <param name="entityClrType"></param>
    /// <returns></returns>
    private LambdaExpression SoftDeleteQueryFilterExpression(Type entityClrType)
    {
        var parameter = Expression.Parameter(entityClrType, "entity");
        var member = Expression.Property(parameter, "IsDeleted");
        var body = Expression.Equal(member, Expression.Constant(0));
        return Expression.Lambda(body, parameter);
    }
}