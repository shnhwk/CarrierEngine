using CarrierEngine.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Domain;

public sealed class CarrierDependencies
{
    public required IHttpClientWrapper Http { get; init; }
    public required ICarrierConfigManager Config { get; init; }
    public required ITrackingStatusMapper TrackingMap { get; init; }
    public required ILoggerFactory LoggerFactory { get; init; }
}