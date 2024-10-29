using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H.LowCode.EntityFrameworkCore;

internal static class ReflectionExtensions
{
    /// <summary>
    /// 得到一个实际的类型（排除Nullable类型的影响）。
    /// </summary>
    /// <param name="type"></param>
    /// <remarks>
    /// 比如：int? 最后将得到int.
    /// </remarks>
    /// <returns></returns>
    public static Type GetRealType(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        if (type.IsGenericType)
            return Nullable.GetUnderlyingType(type) ?? type;
        else
            return type;
    }

    public static bool IsSimpleType(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        return type.IsPrimitive
               || type == typeof(string)
               || type == typeof(DateTime)
               || type == typeof(decimal)
               || type == typeof(Guid)
               || type.IsEnum;
    }


    public static bool IsNullableType(this Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        return type.IsGenericType
               && type.IsGenericTypeDefinition == false
               && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }


    public static bool HasReturn(this MethodInfo m)
    {
        return m.ReturnType != typeof(void);
    }

    /// <summary>
    /// 判断type是否继承或者实现generic
    /// 泛型处理
    /// </summary>
    /// <param name="type"></param>
    /// <param name="generic"></param>
    /// <returns></returns>
    public static bool HasImplemented(this Type type, Type generic)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        if (generic == null) throw new ArgumentNullException(nameof(generic));

        //接口。
        var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
        if (isTheRawGenericType) return true;

        //类型。
        while (type != null && type != typeof(object))
        {
            isTheRawGenericType = IsTheRawGenericType(type);
            if (isTheRawGenericType) return true;
            type = type.BaseType;
        }

        // 没有找到任何匹配的接口或类型。
        return false;

        //某个类型是否是指定的原始接口。
        bool IsTheRawGenericType(Type test)
            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
    }
}
