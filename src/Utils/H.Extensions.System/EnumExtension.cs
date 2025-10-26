using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System;

public static class EnumExtensions
{
    public static string GetEnumName<T>(this T value) where T : Enum
    {
        string key = Enum.GetName(typeof(T), value);
        return key;
    }

    public static string GetEnumName<T>(this int value) where T : Enum
    {
        T enumValue = (T)Enum.ToObject(typeof(T), value);
        string key = enumValue.GetEnumName();
        return key;
    }

    public static string GetEnumName<T>(this string value) where T : Enum
    {
        T enumValue = (T)Enum.Parse(typeof(T), value);
        string key = enumValue.GetEnumName();
        return key;
    }

    public static T GetEnum<T>(this int value) where T : Enum
    {
        T enumValue = (T)Enum.ToObject(typeof(T), value);
        return enumValue;
    }

    public static T GetEnum<T>(this string value) where T : Enum
    {
        T enumValue = (T)Enum.Parse(typeof(T), value);
        return enumValue;
    }
}
