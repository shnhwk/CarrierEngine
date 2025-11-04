using System;
using System.Text.Json.Serialization;

namespace CarrierEngine.Integrations.Carriers.DaytonFreight.Dtos.Tracking;

public class Status
{
    [JsonPropertyName("serviceCenter")]
    public ServiceCenter? ServiceCenter { get; set; }

    [JsonPropertyName("activityCode")]
    public string? ActivityCode { get; set; }

    [JsonPropertyName("activity")]
    public string? Activity { get; set; }

    [JsonPropertyName("trailer")]
    public string? Trailer { get; set; }

    [JsonPropertyName("signedBy")]
    public string? SignedBy { get; set; }

    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }
}