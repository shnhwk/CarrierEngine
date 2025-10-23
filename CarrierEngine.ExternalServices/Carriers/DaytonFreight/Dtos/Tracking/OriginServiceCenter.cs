using System.Text.Json.Serialization;

namespace CarrierEngine.ExternalServices.Carriers.DaytonFreight.Dtos.Tracking;

public class OriginServiceCenter
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("address1")]
    public string Address1 { get; set; }

    [JsonPropertyName("address2")]
    public string Address2 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("fax")]
    public string Fax { get; set; }

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("tollFree")]
    public object TollFree { get; set; }

    [JsonPropertyName("zip")]
    public string Zip { get; set; }
}