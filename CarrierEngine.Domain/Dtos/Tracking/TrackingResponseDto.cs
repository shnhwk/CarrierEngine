namespace CarrierEngine.Domain.Dtos.Tracking;

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
}


public sealed record MappedStatus
{
    public string BanyanCode { get; }
    public bool IsMapped { get; }

    private MappedStatus(string banyanCode, bool isMapped)
    {
        BanyanCode = banyanCode;
        IsMapped = isMapped;
    }

    public static MappedStatus Mapped(string banyanCode) => new(banyanCode, true);
    public static MappedStatus Unmapped => new("SI", false);
}