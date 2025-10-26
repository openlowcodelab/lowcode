using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

public static class TypeExtension
{
    public static object GetDefaultValue(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        //array 
        if (type.IsArray)
            return Array.CreateInstance(type.GetElementType(), 0);

        //List<T>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return Activator.CreateInstance(type);

        //Dictionary<TKey, TValue>
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            return Activator.CreateInstance(type);

        //其他集合接口 
        if (typeof(System.Collections.IList).IsAssignableFrom(type))
            return Array.Empty<object>();

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }
}
