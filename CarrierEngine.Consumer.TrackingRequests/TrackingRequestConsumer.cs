using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
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
                context.Message.BanyanLoadId, DateTime.Now);

            var a = _carrierFactory.GetCarrier<ITracking>(context.Message.CarrierClassName);

            if (a != null)
            {
                var result = await a.TrackLoad(context.Message);
            }



            var returnObject = new TrackingResponseDto()
            {
                BanyanLoadId = context.Message.BanyanLoadId,
                Message = await GetRandomTrackingUpdate()
            };


            _logger.LogInformation("Tracking request for Banyan Load {BanyanLoadId} finished at {ProcessingCompleteDate}.",
                context.Message.BanyanLoadId, DateTime.Now);

        }


        private static async Task<string> GetRandomTrackingUpdate()
        {

            await Task.Delay(TimeSpan.FromSeconds(new Random().Next(0, 4)));

            return new Random().Next(0, 4) switch
            {
                0 => "Load has been picked up",
                1 => "Load in transit",
                2 => "Load is delayed",
                _ => "Load has been delivered",
            };
        }
    }
}