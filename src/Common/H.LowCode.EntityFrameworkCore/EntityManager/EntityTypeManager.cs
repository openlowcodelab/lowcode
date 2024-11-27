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

    public IReadOnlyList<DynamicEntityInfo> LoadDynamicEntities()
    {
        InitDynamicAssembly();

        if(_dynamicEntities.Count > 0)
            return _dynamicEntities;

        var entities = _dataSourceDomainService.GetAllEntities("caseapp");
        foreach (var entity in entities)
        {
            var fields = entity.TableFields.Select(f => new DynamicEntityField()
            {
                Name = f.Name,
                ClrType = FieldTypeMapping.GetFieldType(f.Type, f.IsNullable),
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
