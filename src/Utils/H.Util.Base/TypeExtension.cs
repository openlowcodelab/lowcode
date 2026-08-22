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

    public static Type ResolveType(this string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
        if (type != null)
            return type;

        var (fullName, asmName) = SplitTypeName(typeName);
        if (string.IsNullOrEmpty(fullName))
            return null;

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (!string.IsNullOrEmpty(asmName))
            {
                var name = asm.GetName().Name;
                if (!string.Equals(name, asmName, StringComparison.OrdinalIgnoreCase))
                    continue;
            }
            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: true);
            if (t != null)
                return t;
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName, throwOnError: false, ignoreCase: true);
            if (t != null)
                return t;
        }

        return null;
    }

    private static (string fullName, string asmName) SplitTypeName(string typeName)
    {
        var idx = typeName.IndexOf(',');
        if (idx < 0)
            return (typeName.Trim(), null);
        var fullName = typeName.Substring(0, idx).Trim();
        var asmName = typeName.Substring(idx + 1).Trim();
        return (fullName, asmName);
    }
}
