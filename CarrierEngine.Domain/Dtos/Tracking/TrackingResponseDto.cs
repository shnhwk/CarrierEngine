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
    private bool IsUnMappedCode { get; set; }
}


public class TrackingResponseDto : BaseResponseDto
{
    public TrackingResponseDto()
    {
        TrackingUpdates = [];
    }

    public int BanyanLoadId { get; set; }

    public DateTime? PickupDateTime { get; set; }
    public DateTime? DeliveredDateTime { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }

    public string? ProNumber { get; set; }

    public decimal? CarrierWeight { get; set; }

    public ICollection<TrackingUpdate> TrackingUpdates { get; set; }


    public static TrackingResponseDto Failure(string errorMessage)
    {
        return new TrackingResponseDto
        {
            ErrorMessage = errorMessage,
            IsSuccess = false
        };
    }

    public static TrackingResponseDto Success(int banyanLoadId, ICollection<TrackingUpdate> trackingUpdates,
        string? proNumber = null, DateTime? pickupDateTime = null, DateTime? deliveredDateTime = null, 
        DateTime? estimatedDeliveryDateTime = null, decimal? carrierWeight = null)
    {
        return new TrackingResponseDto
        {
            BanyanLoadId = banyanLoadId,
            ProNumber = proNumber,
            PickupDateTime = pickupDateTime,
            EstimatedDeliveryDate = estimatedDeliveryDateTime,
            DeliveredDateTime = deliveredDateTime,
            TrackingUpdates = trackingUpdates,
            CarrierWeight = carrierWeight,
            IsSuccess = true
        };
    }
}

public abstract class BaseResponseDto
{

    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }

}