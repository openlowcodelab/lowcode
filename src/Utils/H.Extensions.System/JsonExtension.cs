using System;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;

namespace System;

public static class JsonExtension
{
    private static JavaScriptEncoder _defaultEncoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    private static JsonSerializerOptions _defaultOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Encoder = _defaultEncoder
    };

    public static string ToJson(this object obj, JsonSerializerOptions options = null)
    {
        if (obj == null)
            return null;

        if (options != null)
            return JsonSerializer.Serialize(obj, options);

        return JsonSerializer.Serialize(obj, _defaultOptions);
    }

    public static string ToJson(this object obj,
        bool writeIndented,
        JsonIgnoreCondition ignoreCondition = JsonIgnoreCondition.Never,
        JavaScriptEncoder encoder = null)
    {
        if (obj == null)
            return null;

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = writeIndented,
            DefaultIgnoreCondition = ignoreCondition,
            Encoder = encoder ?? _defaultEncoder
        };

        return JsonSerializer.Serialize(obj, options);
    }

    public static T FromJson<T>(this string json, JsonSerializerOptions options = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        if (options != null)
            return JsonSerializer.Deserialize<T>(json, options);

        return JsonSerializer.Deserialize<T>(json, _defaultOptions);
    }

    public static T FromJson<T>(this string json,
        JavaScriptEncoder encoder) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            Encoder = encoder ?? _defaultEncoder
        };

        return JsonSerializer.Deserialize<T>(json, options);
    }

    public static T FromJson<T>(this JsonObject obj, JsonSerializerOptions options = null) where T : class
    {
        if (obj == null)
            return default;

        if (options != null)
            return JsonSerializer.Deserialize<T>(obj, options);

        return JsonSerializer.Deserialize<T>(obj, _defaultOptions);
    }
}
