using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Results;
using CarrierEngine.Domain.Settings;
using CarrierEngine.Infrastructure.Queues;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CarrierEngine.Consumer.TrackingRequests;

public class TrackingJobHandler : ITrackingJobHandler
{
    private readonly ILogger<TrackingJobHandler> _logger;
    private readonly ICarrierFactory _carrierFactory;
    private readonly IRabbitQueuePublisher _rabbitQueuePublisher;
    private readonly RabbitMqOptions _rabbitOptions;

    public TrackingJobHandler(ILogger<TrackingJobHandler> logger, ICarrierFactory carrierFactory, IRabbitQueuePublisher rabbitQueuePublisher, IOptions<RabbitMqOptions> rabbitOptions)
    {
        _logger = logger;
        _carrierFactory = carrierFactory;
        _rabbitQueuePublisher = rabbitQueuePublisher;
        _rabbitOptions = rabbitOptions.Value;
    }

    /// <summary>
    /// Handles a tracking job by retrieving the carrier instance, executing its tracking logic, and enqueueing the result for post-processing.
    /// </summary>
    /// <param name="trackingJob">The <see cref="TrackingJob"/> containing the request data.</param>
    /// <param name="correlationId">The correlation ID for tracing across services.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="MessageHandlerResult{T}"/> containing the tracking response data or an error message and if the message should be ACKed or NACKed with optional requeue.
    /// </returns>
    public async Task<MessageHandlerResult<TrackingResponseDto>> Handle(TrackingJob trackingJob, Guid correlationId, CancellationToken cancellationToken)
    {
        var trackingRequest = trackingJob.Data;

        if (trackingRequest is null)
        {
            _logger.LogWarning("Tracking request data is null for job id {JobId}. Returning failure.", trackingJob.JobId);
            return MessageHandlerResult<TrackingResponseDto>.Failure("Tracking request data is null.");
        }

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CarrierClassName"] = trackingRequest.CarrierClassName,
            ["ProNumber"] = trackingRequest.ProNumber,
            ["BolNumber"] = trackingRequest.BolNumber,
            ["JobId"] = trackingJob.JobId
        });

        _logger.LogInformation("Starting tracking request for load id {BanyanLoadId} with {CarrierClassName} started at {ProcessingStartDateUtc}", trackingRequest.BanyanLoadId, trackingRequest.CarrierClassName, DateTimeOffset.UtcNow);

        ICarrier carrier;
        try
        {
            carrier = await _carrierFactory.GetCarrier(trackingRequest.CarrierClassName, trackingRequest.BanyanLoadId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create carrier instance for {CarrierClassName} for load id {BanyanLoadId}", trackingRequest.CarrierClassName, trackingRequest.BanyanLoadId);
            return MessageHandlerResult<TrackingResponseDto>.Failure($"Failed to create carrier instance for {trackingRequest.CarrierClassName} for load id {trackingRequest.BanyanLoadId}");
        }

        if (carrier is not ITracking tracking)
        {
            _logger.LogWarning("Carrier {CarrierClassName} does not implement ITracking for {BanyanLoadId}. Returning.", trackingRequest.CarrierClassName, trackingRequest.BanyanLoadId);
            return MessageHandlerResult<TrackingResponseDto>.Failure($"Carrier {trackingRequest.CarrierClassName} does not implement ITracking for load id {trackingRequest.BanyanLoadId}");
        }

        try
        {
            await carrier.SetCarrierConfig(trackingRequest.CarrierClassName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve carrier configuration for {CarrierClassName}", trackingRequest.CarrierClassName);
            // *** IMPORTANT: Update Engine Redis/DB Status to FAILED here ***

            return MessageHandlerResult<TrackingResponseDto>.Failure($"Unable to retrieve carrier configuration for {trackingRequest.CarrierClassName} for load id {trackingRequest.BanyanLoadId}");
        }

        try
        {
            await carrier.SetTrackingMaps(trackingRequest.CarrierClassName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to retrieve carrier mappings for {CarrierClassName}", trackingRequest.CarrierClassName);
            // *** IMPORTANT: Update Engine Redis/DB Status to FAILED here ***

            return MessageHandlerResult<TrackingResponseDto>.Failure($"Unable to retrieve carrier mappings for {trackingRequest.CarrierClassName} for load id {trackingRequest.BanyanLoadId}");
        }

        MessageHandlerResult<TrackingResponseDto> response;

        try
        {
            var result = await tracking.TrackLoad(trackingRequest);

            if (result is { IsSuccess: true, Data: not null })
            {
                _logger.LogInformation("Tracking response received from {CarrierClassName} for load id {BanyanLoadId}. Enqueueing to {TrackingPostProcessingQueue} for final processing.", 
                    trackingRequest.CarrierClassName, trackingRequest.BanyanLoadId, _rabbitOptions.TrackingPostProcessingQueue);

                try
                {
                    await _rabbitQueuePublisher.SendToQueueAsync(result.Data, _rabbitOptions.TrackingPostProcessingQueue, correlationId, trackingRequest.BanyanLoadId, cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to enqueue tracking post-processing message for Banyan Load {BanyanLoadId} on queue {QueueName}. Tracking succeeded, but post-processing will be skipped.",
                        trackingRequest.BanyanLoadId, _rabbitOptions.TrackingPostProcessingQueue);
                }

                response = MessageHandlerResult<TrackingResponseDto>.Success(result.Data);
            }
            else
            {
                _logger.LogWarning("Failed to track load {BanyanLoadId} with {CarrierClassName}: {ErrorMessage}", trackingRequest.BanyanLoadId, trackingRequest.CarrierClassName, result.ErrorMessage);
                response = MessageHandlerResult<TrackingResponseDto>.Failure(message: $"This one. Failed to track load id {trackingRequest.BanyanLoadId}");
            }
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid();
            _logger.LogError(ex, "An error occurred tracking load id {BanyanLoadId} with {CarrierClassName}. Error Id: {ErrorId}", trackingRequest.BanyanLoadId, trackingRequest.CarrierClassName, errorId);
            
            response = MessageHandlerResult<TrackingResponseDto>.Failure(message: $"Failed to track load id {trackingRequest.BanyanLoadId}: An error occurred. Error Id: {errorId}");
        }
        finally
        {
            try
            {
                await carrier.SubmitLogs(RequestResponseType.TrackingRequest, cancellationToken);
                _logger.LogDebug("Submitted tracking request/response logs for load id {BanyanLoadId}.", trackingRequest.BanyanLoadId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit tracking r/r logs for load id {BanyanLoadId}", trackingRequest.BanyanLoadId);
            }
        }

        return response;
    }
}