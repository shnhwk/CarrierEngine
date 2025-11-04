using CarrierEngine.Domain.Dtos.Common;
using System.Text.Json.Serialization;

namespace CarrierEngine.Domain.Dtos.Tracking;

/// <summary>
/// Tracking Request Data Transfer Object.
/// </summary>
public class TrackingRequestDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TrackingRequestDto"/> class
    /// with default property values.
    /// </summary>
    public TrackingRequestDto()
    {
        CarrierClassName = string.Empty;
        BolNumber = string.Empty;
        ProNumber = string.Empty;
        PickupNumber = string.Empty;
        CustomerPo = string.Empty;
        Origin = new Address();
        Destination = new Address();
        SubscriptionData = new SubscriptionDetails();
    }

    /// <summary>
    /// The CarrierKey of the carrier class to be used for tracking. This should correspond to the CarrierKey in the Carrier table from the CarrierEngine database.
    /// </summary>
    [JsonPropertyName("carrierClassName")]
    public string CarrierClassName { get; set; }

    /// <summary>
    /// Banyan Load Identifier
    /// </summary>
    [JsonPropertyName("banyanLoadId")]
    public int BanyanLoadId { get; set; }

    /// <summary>
    /// Bill of Lading Number
    /// </summary>
    [JsonPropertyName("bolNumber")]
    public string BolNumber { get; set; }

    /// <summary>
    /// Pro Number, aka ManifestId
    /// </summary>
    [JsonPropertyName("proNumber")]
    public string ProNumber { get; set; }

    /// <summary>
    /// Pickup Number
    /// </summary>
    [JsonPropertyName("pickupNumber")]
    public string PickupNumber { get; set; }

    /// <summary>
    /// Customer Purchase Order Number
    /// </summary>
    [JsonPropertyName("customerPO")]
    public string CustomerPo { get; set; }

    /// <summary>
    /// Origin Address
    /// </summary>
    [JsonPropertyName("origin")]
    public Address Origin { get; set; }

    /// <summary>
    /// Destination Address
    /// </summary>
    [JsonPropertyName("destination")]
    public Address Destination { get; set; }

    /// <summary>
    /// Subscription Details for the carrier account
    /// </summary>
    [JsonPropertyName("subscriptionData")]
    public SubscriptionDetails SubscriptionData { get; set; }
}