using System.Text.Json.Serialization;

namespace CarrierEngine.Integrations.Carriers.DaytonFreight.Dtos.Tracking;

public class Detail
{
    [JsonPropertyName("handlingUnits")]
    public int? HandlingUnits { get; set; }

    [JsonPropertyName("class")]
    public string? Class { get; set; }

    [JsonPropertyName("packaging")]
    public string? Packaging { get; set; }

    [JsonPropertyName("isHazardous")]
    public bool IsHazardous { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("rate")]
    public double? Rate { get; set; }

    [JsonPropertyName("charges")]
    public double? Charges { get; set; }
}