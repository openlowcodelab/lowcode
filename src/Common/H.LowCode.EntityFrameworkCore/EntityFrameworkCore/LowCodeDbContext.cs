using H.LowCode.Domain;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

public class LowCodeDbContext : DbContext
{
    public string AppId { get; private set; }

    private DbContextOptions<LowCodeDbContext> _dbOptions;
    private EntityTypeManager _entityTypeManager;

    public LowCodeDbContext(DbContextOptions<LowCodeDbContext> options,
        EntityTypeManager entityTypeManager) : base(options)
    {
        _dbOptions = options;
        _entityTypeManager = entityTypeManager;
    }

    public async Task<bool> AddAsync(FormEntity formEntity)
    {
        var entityType = GetEntityType(formEntity.Name);
        dynamic entity = Activator.CreateInstance(entityType);

        foreach (var field in formEntity.Fields)
        {
            var propertyInfoName = entityType.GetProperty(field.Key);
            propertyInfoName.SetValue(entity, field.Value);
        }

        Add(entity);
        await SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(FormEntity formEntity)
    {
        var entityType = GetEntityType(formEntity.Name);
        dynamic entity = Activator.CreateInstance(entityType);

        foreach (var field in formEntity.Fields)
        {
            var propertyInfoName = entityType.GetProperty(field.Key);
            propertyInfoName.SetValue(entity, field.Value);
        }

        Update(entity);
        await SaveChangesAsync();
        return true;
    }

    public async Task<FormEntity> GetAsync(string tableName, string id)
    {
        var entityType = GetEntityType(tableName);
        var entity = await FindAsync(entityType, id);
        if (entity == null)
            return null;

        FormEntity formEntity = new FormEntity();
        formEntity.Id = id;
        foreach (var property in entityType.GetProperties())
        {
            var propertyValue = property.GetValue(entity);
            formEntity.Fields[property.Name] = propertyValue;
        }
        return formEntity;
    }

    public int SaveChangesAsync(FormEntity formEntity)
    {
        var entityType = GetEntityType(formEntity.Name);

        var entries = ChangeTracker.Entries().Where(e => e.Entity.GetType() == entityType);
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Deleted)
            {
                // 这里可以添加删除逻辑，比如从数据库中实际删除记录
            }
            else if (entry.State == EntityState.Modified)
            {
                // 这里可以添加更新逻辑，比如更新数据库中的记录
            }
            else if (entry.State == EntityState.Added)
            {
                // 这里可以添加插入逻辑，比如插入新记录到数据库
            }
        }

        return base.SaveChanges();
    }

    public Type GetEntityType(string tableName)
    {
        var entityTypes = this.Model.GetEntityTypes();
        var entityType = entityTypes.FirstOrDefault(t => t.ClrType.Name == tableName);
        if (entityType == null)
            throw new ArgumentException($"Entity type '{tableName}' not found.");
        return entityType.ClrType;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new ReadOnlySaveChangesInterceptor());

        //设置连接字符串
        //optionsBuilder.UseSqlServer("");

        //注册EFcore拦截器，对需要With(NoLock)查询linq进行拦截修改Sql语句
        optionsBuilder.AddInterceptors(new QueryWithNoLockDbCommandInterceptor());
        //表重复注册
        optionsBuilder.ReplaceService<IModelValidator, CustomizeRelationalModelValidator>();
        //Linq查询扩展
        //optionsBuilder.ReplaceService<IRelationalSqlTranslatingExpressionVisitorFactory, CustomizeSqlTranslatingExpressionVisitorFactory>();

        //Entity Attributes填充
        //optionsBuilder.ReplaceService<IInternalEntityEntryFactory, CustomizeEntityEntryFactory>();
        //实体中不可空类型支持
        //optionsBuilder.ReplaceService<IShapedQueryCompilingExpressionVisitorFactory, CustomizeRelationalShapedQueryCompilingExpressionVisitorFactory>();

        base.OnConfiguring(optionsBuilder);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dynamicEntities = _entityTypeManager.LoadDynamicEntities();
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

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureProperties(EntityTypeBuilder entityBuilder, DynamicEntityInfo dynamicEntity, Type entityType)
    {
        foreach (var field in dynamicEntity.Fields)
        {
            if(field.Name == dynamicEntity.PrimaryKey)
            {
                field.MaxLength = 50;
                field.IsUnicode = false;
                field.IsNullable = false;
            }

            //映射字段
            PropertyBuilder propertyBuilder = entityBuilder.Property(field.ClrType, field.Name);

            //映射字段类型
            if (field.ClrType == typeof(string))
            {
                propertyBuilder.HasMaxLength(field.MaxLength ?? 50);
                propertyBuilder.IsUnicode(field.IsUnicode ?? true);
            }
            else if (field.ClrType == typeof(int))
            {
                propertyBuilder.HasMaxLength(field.MaxLength ?? 10);
            }
            else if (field.ClrType == typeof(decimal))
            {
                propertyBuilder.HasPrecision(field.Precision ?? 12, field.Scale ?? 2);
            }

            if (field.DefaultValue != null)
                propertyBuilder.HasDefaultValue(field.DefaultValue);

            if (!field.IsNullable)
                propertyBuilder.IsRequired();

            if (!string.IsNullOrEmpty(field.Comment))
                propertyBuilder.HasComment(field.Comment);
        }
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