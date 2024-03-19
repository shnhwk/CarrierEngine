using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers;
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

            var a = _carrierFactory.GetCarrier<BaseCarrier>(context.Message.CarrierClassName).For(context.Message.BanyanLoadId);
            
            if (a is ITracking tracking)
            {
                var returnObject = new TrackingResponseDto()
                {
                    BanyanLoadId = context.Message.BanyanLoadId,
                };

                try
                {
                    var result = await tracking.TrackLoad(context.Message);
                    returnObject.Message = result.Message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred tracking {BanyanLoadId}", context.Message.BanyanLoadId);
                    returnObject.Message = "Error.";
                }
            }

            _logger.LogInformation(
                "Tracking request for Banyan Load {BanyanLoadId} finished at {ProcessingCompleteDateUtc}.",
                context.Message.BanyanLoadId, DateTimeOffset.UtcNow);
        }
    }
}