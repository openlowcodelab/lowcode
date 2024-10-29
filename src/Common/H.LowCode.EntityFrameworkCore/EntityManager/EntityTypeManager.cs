using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using H.LowCode.Domain;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Volo.Abp.Domain.Entities;

namespace H.LowCode.EntityFrameworkCore;

public class EntityTypeManager
{
    private static AssemblyBuilder _dynamicAssembly;
    private static ModuleBuilder _dynamicModule;
    private const string _dynamicAssemblyName = "H.LowCode.DynamicEntity";
    private const string _dynamicModuleName = "DynamicModule";

    private static List<DynamicEntityInfo> _dynamicEntities = [];

    private IDataSourceDomainService _dataSourceDomainService;

    public EntityTypeManager(IDataSourceDomainService dataSourceDomainService)
    {
        _dataSourceDomainService = dataSourceDomainService;
    }

    public async Task<List<Type>> GetTypesAsync()
    {
        InitDynamicAssembly();

        int i = 1;

        List<Type> types = [];
        var typeName1 = $"tb_person{i++}";
        var entityType1 = EntityFactory.CreateEntityType(_dynamicModule, typeName1, null);
        types.Add(entityType1);

        //var typeName2 = $"tb_test4";
        //var entityTyp2 = EntityFactory.CreateEntityType(_dynamicModule, typeName2, null);
        //types.Add(entityTyp2);

        if (types.Count > 0)
            return types;

        var entities = await _dataSourceDomainService.GetAllEntitiesAsync("caseapp");

        foreach (var entity in entities)
        {
            //定义子类，继承 EntityBase 基类
            var entityType = EntityFactory.CreateEntityType(_dynamicModule, entity.Name, null);
            types.Add(entityType);
            return types;
        }
        return types;
    }

    public async Task<IReadOnlyList<DynamicEntityInfo>> LoadDynamicEntitiesAsync()
    {
        InitDynamicAssembly();

        if(_dynamicEntities.Count > 0)
            return _dynamicEntities;

        var entities = await _dataSourceDomainService.GetAllEntitiesAsync("caseapp");
        foreach (var entity in entities)
        {
            var fields = entity.TableFields.Select(f => new DynamicEntityField()
            {
                Name = f.Name,
                ClrType = FieldTypeMapping.GetFieldType(DbType.SqlServer, f.Type),
                IsNullable = f.IsNullable
            });

            var primaryField = entity.TableFields.FirstOrDefault(t => t.IsPrimaryKey);
            if (primaryField == null)
                throw new ValidationException("primary is required");

            //创建实体类
            var entityType = EntityFactory.CreateEntityType(_dynamicModule, entity.Name, fields);
            
            var dynamicEntity = new DynamicEntityInfo()
            {
                EntityName = entity.Name,
                EntityType = entityType,
                PrimaryKey = primaryField.Name,
                EnableSoftDelete = entity.EnableSoftDelete,
                Fields = fields
            };
            _dynamicEntities.Add(dynamicEntity);
            return _dynamicEntities;
        }
        return _dynamicEntities;
    }

    private static void InitDynamicAssembly()
    {
        if (_dynamicAssembly == null)
        {
            AssemblyName dynamicAssemblyName = new AssemblyName(_dynamicAssemblyName);
            //dynamicAssemblyName.Version = new Version(1, 0, 0, 1);

            _dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
            _dynamicModule = _dynamicAssembly.DefineDynamicModule(_dynamicModuleName);
        }
    }
}
