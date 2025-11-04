using System.Text.Json.Serialization;

namespace CarrierEngine.Integrations.Carriers.DaytonFreight.Dtos.Tracking;

public class DestinationPartner
{
    [JsonPropertyName("pro")]
    public string? Pro { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}