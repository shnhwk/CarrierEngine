using System.Text.Json.Serialization;

namespace CarrierEngine.Integrations.Carriers.ExampleCarrier2.Dtos;


public class TrackingResponse
{
    [JsonPropertyName("datetime")]
    public string? Datetime { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}