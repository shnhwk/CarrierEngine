namespace CarrierEngine.Domain.Dtos.Jobs;

/// <summary>
/// Defines the lifecycle states for engine job processing.
/// </summary>
public enum JobProcessingStatus
{
    Pending = 0,
    InProgress = 1,
    Success = 2,
    Failed = 3,
    Skipped = 4,
    Retrying = 5,
    Cancelled = 6
}