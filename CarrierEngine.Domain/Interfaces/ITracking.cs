using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Results;

namespace CarrierEngine.Domain.Interfaces
{
    public interface ITracking
    {
        public Task<CarrierResult<TrackingResponseDto>> TrackLoad(TrackingRequestDto trackingRequest);
    }
}