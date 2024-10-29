using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using H.LowCode.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace H.LowCode.EntityFrameworkCore;

public class EntityFactory
{
    private static readonly HashSet<string> s_typeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 创建实体类
    /// </summary>
    /// <param name="mb"></param>
    /// <param name="entityName"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static Type CreateEntityType(ModuleBuilder mb, string entityName, IEnumerable<DynamicEntityField> fields)
    {
        if (s_typeNames.Contains(entityName))
        {
            entityName = entityName + "_";
            s_typeNames.Add(entityName);
        }

        //定义子类，继承 EntityBase 基类
        string entityTypeName = $"{mb.Assembly.GetName().Name}.{entityName}";
        var tb = mb.DefineType(entityTypeName, typeof(EntityBase));

        //定义属性
        //foreach (var field in fields)
        //{
        //    //定义属性
        //    tb.DefineField(field.Name, field.ClrType);
        //}

        var entityType = tb.CreateType();
        return entityType;
    }

    /// <summary>
    /// 创建实体对象
    /// </summary>
    /// <typeparam name="T"><see cref="Entity"/>实例的类型。</typeparam>
    /// <returns>实体对象。</returns>
    public static T CreateInstance<T>() where T : EntityBase
    {
        return (T)CreateInstance(typeof(T));
    }

    /// <summary>
    /// 创建实体对象
    /// </summary>
    /// <param name="type"><see cref="Entity"/>实例的类型。</param>
    /// <returns>实体对象。</returns>
    public static EntityBase CreateInstance(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        ConstructorInfo ctorInfo = type.GetConstructor(Type.EmptyTypes);
        Func<object> ctor = CreateInstanceByDelegate(ctorInfo);

        return (EntityBase)ctor.Invoke();
    }

    /// <summary>
    /// 基于委托创建实体对象
    /// </summary>
    /// <param name="constructor"></param>
    /// <returns></returns>
    private static Func<object> CreateInstanceByDelegate(ConstructorInfo constructor)
    {
        DynamicMethod dynamicMethod = new DynamicMethod("ctor", constructor.DeclaringType, Type.EmptyTypes, true);

        ILGenerator il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Nop);
        il.Emit(OpCodes.Newobj, constructor);
        il.Emit(OpCodes.Ret);

        return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
    }
}
