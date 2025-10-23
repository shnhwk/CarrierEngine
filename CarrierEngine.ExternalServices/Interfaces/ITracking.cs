using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos.Tracking;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface ITracking
{
    public Task<TrackingResponseDto> TrackLoad(TrackingRequestDto trackingRequest);
}