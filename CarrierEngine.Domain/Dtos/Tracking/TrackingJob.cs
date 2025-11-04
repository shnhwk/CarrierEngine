using System.Text.Json.Serialization;
using CarrierEngine.Domain.Interfaces;

namespace CarrierEngine.Domain.Dtos.Tracking;

public record TrackingJob : IEngineJob
{
    [JsonPropertyName("jobId")]
    public Guid JobId { get; set; }

    public int? BanyanLoadId { get; set; }

    [JsonPropertyName("data")]
    public TrackingRequestDto? Data { get; init; }
}