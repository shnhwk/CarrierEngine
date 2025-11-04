using RabbitMQ.Client;

namespace CarrierEngine.Infrastructure.Queues;

public interface IRabbitConnectionFactory
{
    Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}