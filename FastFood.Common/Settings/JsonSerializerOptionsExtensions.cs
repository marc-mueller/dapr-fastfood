using System.Text.Json;
using System.Text.Json.Serialization;

namespace FastFood.Common.Settings;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ConfigureJsonSerializerOptions(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // Ensure consistency with common JSON conventions
        return options;
    } 
}