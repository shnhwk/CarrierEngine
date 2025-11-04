using System;
using System.Net;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos.Jobs;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Settings;
using CarrierEngine.Infrastructure.Queues;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarrierEngine.Producer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrackingRequestController : ControllerBase
{
    private readonly ICorrelationContext _correlation;
    private readonly ILogger<TrackingRequestController> _logger;
    private readonly IRabbitQueuePublisher _queuePublisher;
    private readonly IJobStatusUpdater _jobStatusUpdater;
    private readonly RabbitMqOptions _rabbitMqOptions;

    public TrackingRequestController(ICorrelationContext correlation, ILogger<TrackingRequestController> logger, IRabbitQueuePublisher queuePublisher, 
        IJobStatusUpdater jobStatusUpdater, IOptions<RabbitMqOptions> rabbitMqOptions)
    {
        _correlation = correlation;
        _logger = logger;
        _queuePublisher = queuePublisher;
        _jobStatusUpdater = jobStatusUpdater;
        _rabbitMqOptions = rabbitMqOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Post(TrackingRequestDto trackingRequestDto)
    {
        if (trackingRequestDto is null)
            return BadRequest();

        try
        {
            var jobId = Guid.NewGuid();
            var trackingJob = new TrackingJob
            {
                JobId = jobId,
                BanyanLoadId = trackingRequestDto.BanyanLoadId,
                Data = trackingRequestDto
            };

            _logger.LogInformation("Sending {@trackingRequestDto} to {RabbitMqConstants.TrackingRequestQueue} with JobId {JobId}", trackingRequestDto, _rabbitMqOptions.TrackingQueue, jobId);

            //TODO: this prob needs to be in a service with some retries in case one or the other fails. 
            await _jobStatusUpdater.UpdateJobStatusAsync(jobId, trackingRequestDto.BanyanLoadId, JobProcessingStatus.Pending);
            await _queuePublisher.SendToQueueAsync(trackingJob, _rabbitMqOptions.TrackingQueue, _correlation.Get(), trackingRequestDto.BanyanLoadId);

            return Accepted(new { JobId = jobId, Status = "PENDING" });  //TODO response object? 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to send {@trackingRequestDto} to {RabbitMqConstants.TrackingRequestQueue}", trackingRequestDto, _rabbitMqOptions.TrackingQueue);
            return StatusCode((int)HttpStatusCode.InternalServerError, "Unable to send message to queue.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid jobId)
    {
        var jobStatus = await _jobStatusUpdater.GetJobStatusAsync(jobId);

        if (jobStatus is null)
            return NotFound();

        return Ok(jobStatus);
    } 
}