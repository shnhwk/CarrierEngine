 
using System.Text.Json.Serialization;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.ConfigurationData;

namespace CarrierEngine.ExternalServices.Carriers.DaytonFreight.Config;


public class DaytonFreightConfig : BaseConfigData
{
    [JsonPropertyName("defaultTimeoutSeconds")]
    public int DefaultTimeoutSeconds { get; set; }

    [JsonPropertyName("prodBaseUrl")]
    public string ProdBaseUrl { get; set; }

    [JsonPropertyName("sandboxBaseUrl")]
    public string SandboxBaseUrl { get; set; }

    [JsonPropertyName("authEndpoint")]
    public string AuthEndpoint { get; set; }

    [JsonPropertyName("trackingEndpoint")]
    public string TrackingEndpoint { get; set; }

    [JsonPropertyName("ratingEndpoint")]
    public string RatingEndpoint { get; set; }

    [JsonPropertyName("dispatchEndpoint")]
    public string DispatchEndpoint { get; set; }

    [JsonPropertyName("useSandbox")]
    public bool UseSandbox { get; set; }
}

