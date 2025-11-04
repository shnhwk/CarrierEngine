using CarrierEngine.Domain.Dtos.Tracking;

namespace CarrierEngine.Domain.Interfaces;

public interface ITrackingStatusMapper
{
    MappedStatus MapCode(string rawStatus);

    Task LoadMappings(string carrierKey, CancellationToken ct = default);
}