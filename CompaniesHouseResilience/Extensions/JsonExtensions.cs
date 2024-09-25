using System.Text.Json;

namespace CompaniesHouseResilience.Extensions;

public static class JsonExtensions
{
    public static string? GetStringFromJsonElement(
            this JsonElement element,
            string propertyName,
            string? defaultValue = default) =>
        element.ValueKind != JsonValueKind.Undefined
                     && element.TryGetProperty(propertyName, out var property)
                     && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : defaultValue;
}
