using System.Text.Json;
using System.Text.Json.Serialization;

namespace CarrierEngine.Domain;

public class NullableDateTimeConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (DateTime.TryParse(str, out var date))
                return date == DateTime.MinValue ? null : date;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value);
        else
            writer.WriteNullValue();
    }
}