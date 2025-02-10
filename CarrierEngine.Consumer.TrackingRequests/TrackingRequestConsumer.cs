using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices;
using CarrierEngine.ExternalServices.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Consumer.TrackingRequests
{
    public class TrackingRequestConsumer : IConsumer<TrackingRequestDto>
    {
        private readonly ILogger<TrackingRequestConsumer> _logger;
        private readonly ICarrierFactory _carrierFactory;

        public TrackingRequestConsumer(ILogger<TrackingRequestConsumer> logger, ICarrierFactory carrierFactory)
        {
            _logger = logger;
            _carrierFactory = carrierFactory;
        }

        public async Task Consume(ConsumeContext<TrackingRequestDto> context)
        {
            _logger.LogInformation("Tracking request for Banyan Load {BanyanLoadId} started at {ProcessingStartDate}", context.Message.BanyanLoadId, DateTimeOffset.UtcNow);

            var a = (await _carrierFactory.GetCarrier(context.Message.CarrierClassName));

            await a.SetCarrierConfig(context.Message.CarrierClassName);



            if (a is not ITracking tracking)
            {
                _logger.LogWarning(
                    "Carrier {CarrierClassName} does not implement ITracking for {BanyanLoadId}. Returning.",
                    context.Message.CarrierClassName, context.Message.BanyanLoadId);

                return;
            }

            try
            {
                var result = await tracking.TrackLoad(context.Message);

                _logger.LogDebug("Tracking result for Banyan Load {BanyanLoadId}: {ResultMessage}", context.Message.BanyanLoadId, result.Message);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred tracking {BanyanLoadId}", context.Message.BanyanLoadId);
            }
            finally
            {
                try
                {
                    await a.SubmitLogs(RequestResponseType.TrackingRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to submit logs for {BanyanLoadId}", context.Message.BanyanLoadId);
                }
            }

            _logger.LogInformation(
                "Tracking request for Banyan Load {BanyanLoadId} finished at {ProcessingCompleteDateUtc}.",
                context.Message.BanyanLoadId, DateTimeOffset.UtcNow);
        }
    }
}