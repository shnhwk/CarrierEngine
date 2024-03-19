using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices.Carriers.ExampleCarrier2.Dtos;

public class AuthResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}