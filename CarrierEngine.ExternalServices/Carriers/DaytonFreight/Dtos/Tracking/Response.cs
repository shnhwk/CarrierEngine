using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices.Carriers.DaytonFreight.Dtos.Tracking;

public class Response
{
    [JsonPropertyName("results")]
    public List<Result> Results { get; set; }
}