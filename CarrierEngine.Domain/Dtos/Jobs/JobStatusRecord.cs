namespace CarrierEngine.Domain.Dtos.Jobs;

/// <summary>
/// Represents the persisted or cached state of a job's progress.
/// </summary>
public sealed record JobStatusRecord(
    Guid JobId,
    int? BanyanLoadId,
    string CarrierClassName,
    JobProcessingStatus Status,
    string? Message,
    DateTimeOffset LastUpdatedUtc);