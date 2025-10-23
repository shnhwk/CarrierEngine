using System.Text.Json;

namespace CarrierEngine.ExternalServices;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new NullableDateTimeConverter() }
    };
}