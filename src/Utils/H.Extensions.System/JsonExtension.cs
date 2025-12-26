using System.Text.Encodings.Web;
using System.Text.Json;
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

    public static string? ToJson(this object? obj, JsonSerializerOptions? options = null)
    {
        if (obj == null)
            return null;

        if (options != null)
            return JsonSerializer.Serialize(obj, options);

        return JsonSerializer.Serialize(obj, _defaultOptions);
    }

    public static T? FromJson<T>(this string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, _defaultOptions);
        }
        catch (JsonException ex)
        {
            throw new JsonException($"反序列化异常: message={ex.Message}, path={ex.Path}, json={json}");
        }
        catch (Exception ex)
        {
            throw new JsonException($"反序列化异常: message={ex.Message}, json={json}", ex);
        }
    }
}
