using CarrierEngine.Domain.Dtos.Jobs;
using CarrierEngine.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Infrastructure.Jobs;

public class JobStatusUpdater : IJobStatusUpdater
{
    private readonly ILogger<JobStatusUpdater> _logger;

    public JobStatusUpdater(ILogger<JobStatusUpdater> logger)
    {
        _logger = logger;
    }

    public async Task UpdateJobStatusAsync(Guid jobId, int? loadId, JobProcessingStatus status, string? message = null,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Updating job status: JobId={JobId}, LoadId={LoadId}, Status={Status}, Message={Message}", jobId, loadId, status, message ?? "N/A");
    }

    public async Task<JobStatusRecord?> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        _logger.LogInformation("Retrieving job status for JobId={JobId}", jobId);
        return new JobStatusRecord(jobId, 1234, "", JobProcessingStatus.Success, "", DateTimeOffset.UtcNow);
    }
}