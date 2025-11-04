using System.Text.Json;
using RabbitMQ.Client;

namespace CarrierEngine.Infrastructure.Queues;

/// <summary>
/// Provides functionality to publish messages to RabbitMQ queues with optional queue creation
/// and structured message properties, including correlation IDs and load identifiers.
/// </summary>
public sealed class RabbitQueuePublisher : IRabbitQueuePublisher, IAsyncDisposable, IDisposable
{
    private readonly IRabbitConnectionFactory _connectionFactory;

    private IConnection? _connection;
 
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitQueuePublisher"/> class using the specified connection factory.
    /// </summary>
    /// <param name="connectionFactory">The <see cref="IRabbitConnectionFactory"/> used to create RabbitMQ connections.</param>
    public RabbitQueuePublisher(IRabbitConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Sends a message to a specified RabbitMQ queue, optionally ensuring that the queue exists.
    /// The message is serialized as JSON and published with persistent delivery and custom headers.
    /// </summary>
    /// <typeparam name="T">The type of the message to serialize and send.</typeparam>
    /// <param name="message">The message to send.</param>
    /// <param name="queueName">The name of the target RabbitMQ queue.</param>
    /// <param name="correlationId">A unique correlation identifier for tracing the message.</param>
    /// <param name="banyanLoadId">An optional load identifier to include in the message headers.</param>
    /// <param name="ensureQueue">
    /// If <c>true</c>, the queue will be declared if it does not exist. Defaults to <c>true</c>.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous publish operation.</returns>
    public async Task SendToQueueAsync<T>(T message, string queueName, Guid correlationId, int banyanLoadId, bool ensureQueue = true, CancellationToken cancellationToken = default)
    {
        if (_connection is null || !_connection.IsOpen)
        {
            _connection?.Dispose();
            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken).ConfigureAwait(false);
        }

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true
        );

        await using var channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken)
            .ConfigureAwait(false);

        if (ensureQueue)
        {
            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                noWait: false,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        var body = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        var props = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent, // 2
            CorrelationId = correlationId.ToString(),
            MessageId = Guid.NewGuid().ToString("N"), 
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["BanyanLoadId"] = banyanLoadId
            }
        };
 
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Disposes the underlying RabbitMQ connection.
    /// </summary>
    public void Dispose() => _connection?.Dispose();

    /// <summary>
    /// Asynchronously disposes the underlying RabbitMQ connection.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        _connection?.Dispose();
        return ValueTask.CompletedTask;
    }
}