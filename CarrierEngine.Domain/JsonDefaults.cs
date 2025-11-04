using System.Text.Json;

namespace CarrierEngine.Domain;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new NullableDateTimeConverter() }
    };
}