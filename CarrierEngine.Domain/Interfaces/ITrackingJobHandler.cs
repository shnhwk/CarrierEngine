using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Results;

namespace CarrierEngine.Domain.Interfaces;

public interface ITrackingJobHandler
{
    Task<MessageHandlerResult<TrackingResponseDto>> Handle(TrackingJob job, Guid correlationId, CancellationToken ct);
}