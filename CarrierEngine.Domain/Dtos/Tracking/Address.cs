using System.Text.Json.Serialization;

namespace CarrierEngine.Domain.Dtos.Tracking;

/// <summary>
/// Represents a physical address, supporting international regions and countries.
/// </summary>
public class Address
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class. />.
    /// </summary>
    public Address()
    {
        Address1 = string.Empty;
        City = string.Empty;
        Region = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    /// <summary>
    /// Gets or sets the first line of the address.
    /// </summary>
    [JsonPropertyName("address1")]
    public string Address1 { get; set; }

    /// <summary>
    /// Gets or sets the city or locality.
    /// </summary>
    [JsonPropertyName("city")]
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the state, province, or region of the address.
    /// </summary>
    [JsonPropertyName("region")]
    public string Region { get; set; }

    /// <summary>
    /// Gets or sets the postal or ZIP code.
    /// </summary>
    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; }

    /// <summary>
    /// Gets or sets the country or territory.
    /// </summary>
    [JsonPropertyName("country")]
    public string Country { get; set; }
}