using System.Text.Json;
using System.Text.Json.Serialization;

namespace CrossService.IntegrationTests.Common;

public static class JsonSerializerHelper
{
    public static JsonSerializerOptions CamelCaseOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static JsonSerializerOptions RequestOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
        // Enums serializados como int (padrão)
    };

    public static string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj, CamelCaseOptions);

    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, CamelCaseOptions);
}
