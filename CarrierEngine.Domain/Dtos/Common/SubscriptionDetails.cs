using System.Text.Json.Serialization;

namespace CarrierEngine.Domain.Dtos.Common;

/// <summary>
/// Banyan subscription or credential information used when communicating with a carrier's API or web portal.
/// </summary>
public class SubscriptionDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionDetails"/> class with default property values.
    /// </summary>
    public SubscriptionDetails()
    {
        ApiUsername = string.Empty;
        ApiPassword = string.Empty;
        WebUserName = string.Empty;
        WebPassword = string.Empty;
        AccountId = string.Empty;
        AccountNote = string.Empty;
        Miscellaneous = string.Empty;
    }

    /// <summary>
    /// Gets or sets the API username for carrier authentication.
    /// </summary>
    [JsonPropertyName("apiUsername")]
    public string ApiUsername { get; set; }

    /// <summary>
    /// Gets or sets the API password for carrier authentication.
    /// </summary>
    [JsonPropertyName("apiPassword")]
    public string ApiPassword { get; set; }

    /// <summary>
    /// Gets or sets the web portal username for carrier authentication.
    /// </summary>
    [JsonPropertyName("webUserName")]
    public string WebUserName { get; set; }

    /// <summary>
    /// Gets or sets the web portal password for carrier authentication.
    /// </summary>
    [JsonPropertyName("webPassword")]
    public string WebPassword { get; set; }

    /// <summary>
    /// Gets or sets the carrier account identifier.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    /// <summary>
    /// Gets or sets additional notes related to the carrier account.
    /// </summary>
    [JsonPropertyName("accountNote")]
    public string AccountNote { get; set; }

    /// <summary>
    /// Gets or sets miscellaneous information for carrier communication.
    /// </summary>
    [JsonPropertyName("miscellaneous")]
    public string Miscellaneous { get; set; }
}