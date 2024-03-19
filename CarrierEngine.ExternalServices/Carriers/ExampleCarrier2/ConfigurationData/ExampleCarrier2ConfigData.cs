using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.ConfigurationData;

public class ExampleCarrier2ConfigData
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

