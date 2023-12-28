using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier2;
using CarrierEngine.ExternalServices.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Consumer.TrackingRequests
{
    public class TrackingRequestConsumer : IConsumer<TrackingRequestDto>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TrackingRequestConsumer> _logger;
        private readonly ICarrierFactory _carrierFactory;

        public TrackingRequestConsumer(IServiceProvider serviceProvider, ILogger<TrackingRequestConsumer> logger, ICarrierFactory carrierFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _carrierFactory = carrierFactory;
        }

        public async Task Consume(ConsumeContext<TrackingRequestDto> context)
        {

            _logger.LogInformation("Tracking request for Banyan Load {BanyanLoadId} started at {ProcessingStartDate}",
                context.Message.BanyanLoadId, DateTimeOffset.UtcNow);

            var a = _carrierFactory.GetCarrier<BaseCarrier>(context.Message.CarrierClassName);

            if (a is ITracking tracking)
            { 
                var result = await tracking.TrackLoad(context.Message);

                var returnObject = new TrackingResponseDto()
                {
                    BanyanLoadId = context.Message.BanyanLoadId,
                    Message = result.Message
                };
            }

       

            _logger.LogInformation("Tracking request for Banyan Load {BanyanLoadId} finished at {ProcessingCompleteDateUtc}.",
                context.Message.BanyanLoadId, DateTimeOffset.UtcNow);

        }
    }
}