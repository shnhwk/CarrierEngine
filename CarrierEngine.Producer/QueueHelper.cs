using CarrierEngine.Domain;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace CarrierEngine.Producer
{
    public static class QueueHelper
    {
        public static async Task SentToMessageQueue<T>(T objectToSend, string queueName, IBus bus, Guid correlationId, int banyanLoadId)
        {

            var url = RabbitMqConstants.RabbitMqRootUri + (RabbitMqConstants.RabbitMqRootUri.EndsWith("/") ? "" : "/") + queueName;

            var uri = new Uri(url);
            var endPoint = await bus.GetSendEndpoint(uri);

            await endPoint.Send(objectToSend, context =>
            {
                context.Headers.Set("BanyanLoadId", banyanLoadId);
                context.CorrelationId = correlationId;
            });
        }
    }
}
