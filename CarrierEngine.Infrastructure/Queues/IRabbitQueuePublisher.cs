namespace CarrierEngine.Infrastructure.Queues;

public interface IRabbitQueuePublisher
{
    Task SendToQueueAsync<T>(T message, string queueName, Guid correlationId, int banyanLoadId, bool ensureQueue = true, CancellationToken cancellationToken = default);
}