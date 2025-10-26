using System;
using System.Collections.Concurrent;

namespace System;

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> _enumNameCache = new();

    public static string GetEnumName<T>(this T value) where T : Enum
    {
        return _enumNameCache.GetOrAdd(value, v => Enum.GetName(typeof(T), v) ?? string.Empty);
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

    public static T ToEnum<T>(this int value, T defaultValue = default) where T : struct, Enum
    {
        return Enum.IsDefined(typeof(T), value) ? (T)Enum.ToObject(typeof(T), value) : defaultValue;
    }

    public static T ToEnum<T>(this int? value, T defaultValue = default) where T : struct, Enum
    {
        if (!value.HasValue) return defaultValue;
        return value.Value.ToEnum(defaultValue);
    }

    public static T ToEnum<T>(this string value, T defaultValue = default) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        return Enum.TryParse<T>(value, true, out T result) ? result : defaultValue;
    }
}
