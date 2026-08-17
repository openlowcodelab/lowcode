using System.Text.Json;

namespace System;

/// <summary>
/// Object 对象扩展
/// </summary>
public static class ObjectExtension
{
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <returns></returns>
    public static T? DeepClone<T>(this T source) where T : class
    {
        var jsonString = source.ToJson();
        var result = jsonString.FromJson<T>();
        return result;
    }

    public static object? ConvertToRealType(this object? obj, Type targetType)
    {
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        if (obj == null)
        {
            return targetType.GetDefaultValue();
        }

        if (targetType.IsInstanceOfType(obj))
        {
            return obj;
        }

        // 处理 JsonElement 类型
        string stringValue = obj.ToString();
        if (obj is JsonElement jsonElement)
        {
            // 尝试获取字符串值
            if (jsonElement.ValueKind == JsonValueKind.String)
            {
                stringValue = jsonElement.GetString() ?? string.Empty;
            }
            else if (jsonElement.ValueKind == JsonValueKind.Number)
            {
                stringValue = jsonElement.GetRawText();
            }
        }

        try
        {
            Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // 处理枚举类型
            if (underlyingType.IsEnum)
            {
                return Enum.Parse(underlyingType, stringValue);
            }

            var value = Convert.ChangeType(stringValue, underlyingType);
            return value;
        }
        catch
        {
            return null;
        }
    }

    public static object? ConvertToRealType(this JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out int intValue) ? intValue : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Object => ConvertJsonObjectToDictionary(element),
            JsonValueKind.Array => ConvertJsonArrayToList(element),
            _ => throw new NotSupportedException($"Unsupported JsonValueKind: {element.ValueKind}")
        };
    }

    private static Dictionary<string, object?> ConvertJsonObjectToDictionary(JsonElement element)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var property in element.EnumerateObject())
        {
            dict[property.Name] = property.Value.ConvertToRealType();
        }
        return dict;
    }

    private static List<object?> ConvertJsonArrayToList(JsonElement element)
    {
        var list = new List<object?>();
        foreach (var item in element.EnumerateArray())
        {
            list.Add(item.ConvertToRealType());
        }
        return list;
    }
}
