using System;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos;
using MassTransit;

namespace CarrierEngine.Consumer.TrackingRequests
{
    public class TrackingRequestConsumerNotification : IConsumer<TrackingRequestDto>
    {
        public async Task Consume(ConsumeContext<TrackingRequestDto> context)
        {
            await Console.Out.WriteLineAsync(
                $"Tracking request for Banyan Load {context.Message.BanyanLoadId} started at {DateTime.Now}");

            var sleepTime = new Random().Next(500, 5000);
            await Task.Delay(sleepTime);

            var returnObject = new TrackingResponseDto()
            {
                BanyanLoadId = context.Message.BanyanLoadId,
                Message = GetRandomTrackingUpdate()
            };

            await Console.Out.WriteLineAsync(
                $"Tracking request for Banyan Load {returnObject.BanyanLoadId} finished at {DateTime.Now}. {returnObject.Message}. Simulated work time {sleepTime}ms.");
        }


        private static string GetRandomTrackingUpdate()
        {
            switch (new Random().Next(0, 4))
            {
                case 0:
                    return "Load has been picked up";
                case 1:
                    return "Load in transit";
                case 2:
                    return "Load is delayed";
                default:
                    return "Load has been delivered";
            }
        }
    }
}