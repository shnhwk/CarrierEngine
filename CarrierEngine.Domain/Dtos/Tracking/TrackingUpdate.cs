namespace CarrierEngine.Domain.Dtos.Tracking;

public class TrackingUpdate
{
    public TrackingUpdate()
    {
        CapturedDateTime = DateTime.Now;
        CarrierMessage = string.Empty;
        CarrierCode = string.Empty;
    }

    /// <summary>
    /// Gets or sets the location associated with the tracking update.
    /// </summary>
    public Address? Location { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the carrier reported the status update.
    /// </summary>
    public DateTime? CarrierStatusTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the tracking update was captured in the Banyan System.
    /// </summary>
    public DateTime CapturedDateTime { get; set; }

    /// <summary>
    /// Gets or sets the carrier's status message.
    /// </summary>
    public string CarrierMessage { get; set; }

    /// <summary>
    /// Gets or sets the carrier's status code.
    /// </summary>
    public string CarrierCode { get; set; }

    /// <summary>
    /// Gets or sets the mapped Banyan status code.
    /// </summary>
    public string? BanyanCode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the carrier code was unmapped.
    /// </summary>
    public bool IsUnMappedCode { get; set; }
}