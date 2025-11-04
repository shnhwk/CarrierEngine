using CarrierEngine.Domain.Dtos.Jobs;

namespace CarrierEngine.Domain.Interfaces;

/// <summary>
/// Provides a mechanism for updating the processing status of engine jobs
/// (e.g., tracking, rating, dispatch) in both Redis and/or the database.
/// </summary>
public interface IJobStatusUpdater
{
    /// <summary>
    /// Updates the current status of a job in cache and/or persistent storage.
    /// </summary>
    /// <param name="jobId">The unique ID of the job (usually a GUID or long).</param>
    /// <param name="loadId">The Banyan load ID associated with this job, if available.</param>
    /// <param name="status">The new status value to set.</param>
    /// <param name="message">An optional message or reason (e.g. "Config load failed").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateJobStatusAsync(
        Guid jobId,
        int? loadId,
        JobProcessingStatus status,
        string? message = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current status for a specific job.
    /// </summary>
    /// <param name="jobId">The unique job ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current job status record, or null if not found.</returns>
    Task<JobStatusRecord?> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
}