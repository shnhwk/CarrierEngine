using CarrierEngine.Domain.Dtos.Jobs;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CarrierEngine.Infrastructure.Queues;


/// <summary>
/// Base class for background services that consume messages from a RabbitMQ queue.
/// Provides automatic connection management, message deserialization, logging, 
/// and acknowledgment/NACK handling.
/// </summary>
/// <typeparam name="TMessage">The type of the message to consume.</typeparam>
/// <typeparam name="TResult">The type returned by the message handler.</typeparam>
public abstract class RabbitWorkerBase<TMessage, TResult> : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IRabbitConnectionFactory _connFactory;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _queueName;
    private readonly Func<byte[], (bool ok, TMessage? msg, string? reason)> _deserialize;

    private IConnection? _connection;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitWorkerBase{TMessage, TResult}"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> for logging consumer events.</param>
    /// <param name="connFactory">The <see cref="IRabbitConnectionFactory"/> for creating RabbitMQ connections.</param>
    /// <param name="scopeFactory">The <see cref="IServiceScopeFactory"/> for creating scoped service providers.</param>
    /// <param name="queueName">The name of the RabbitMQ queue to consume.</param>
    /// <param name="deserialize">
    /// A function that takes a byte array and returns a tuple indicating whether deserialization succeeded,
    /// the deserialized message (if any), and a reason string if deserialization failed.
    /// </param>
    protected RabbitWorkerBase(ILogger logger, IRabbitConnectionFactory connFactory, IServiceScopeFactory scopeFactory,
        string queueName, Func<byte[], (bool ok, TMessage? msg, string? reason)> deserialize)
    {
        _logger = logger;
        _connFactory = connFactory;
        _scopeFactory = scopeFactory;
        _queueName = queueName;
        _deserialize = deserialize;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumerAsync(stoppingToken);
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Queue}] Consumer crashed; retrying in 5s…", _queueName);
                await StopConsumerAsync();
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
        await StopConsumerAsync();
    }

    /// <summary>
    /// Starts the RabbitMQ consumer and attaches the event handler.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    private async Task StartConsumerAsync(CancellationToken ct)
    {
        _connection = await _connFactory.CreateConnectionAsync(ct);

        var chanOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: false,
            publisherConfirmationTrackingEnabled: false,
            outstandingPublisherConfirmationsRateLimiter: null,
            consumerDispatchConcurrency: 1 //TODO: _options.ConsumerDispatchConcurrency  config drive, this is how many it will process at 1 time.
        );

        _channel = await _connection.CreateChannelAsync(chanOptions, ct);

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true, exclusive: false, autoDelete: false,
            arguments: null, noWait: false, cancellationToken: ct);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 8, global: false, cancellationToken: ct);

        _consumer = new AsyncEventingBasicConsumer(_channel);
        _consumer.ReceivedAsync += OnReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: _consumer,
            cancellationToken: ct);

        _logger.LogInformation("[{Queue}] Consuming.", _queueName);
    }

    /// <summary>
    /// Stops the RabbitMQ consumer and disposes the channel and connection.
    /// </summary>
    private async Task StopConsumerAsync()
    {
        try
        {
            if (_consumer is not null)
                _consumer.ReceivedAsync -= OnReceivedAsync;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error detaching consumer event.");
        }

        try
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync(replyCode: 200, replyText: "Shutting down").ConfigureAwait(false);
                await _channel.DisposeAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing channel asynchronously.");
        }

        try
        {
            if (_connection is not null)
            {
                await _connection.CloseAsync(reasonCode: 200, reasonText: "Shutting down").ConfigureAwait(false);
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing connection asynchronously.");
        }

        _consumer = null;
        _channel = null;
        _connection = null;
    }

    /// <summary>
    /// Handles messages received from RabbitMQ, performing deserialization, logging, job status updates,
    /// and acknowledgment or negative acknowledgment.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="ea">The <see cref="BasicDeliverEventArgs"/> containing the message data.</param>
    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            if (!Guid.TryParse(ea.BasicProperties.CorrelationId, out var correlationId))
            {
                //TODO: account for correlation ID being null or invalid, especially in the BeginMessageScope below
                _logger.LogWarning("[{Queue}] Message missing or invalid CorrelationId; generating new one.", _queueName);

                correlationId = Guid.NewGuid();
            }

            var (ok, msg, reason) = _deserialize(ea.Body.ToArray());
            if (!ok || msg is null)
                throw new InvalidOperationException($"Deserialization failed: {reason ?? "unknown"}");

            MessageHandlerResult<TResult> result;
            var meta = msg as IEngineJob;

            using (RabbitMessageLogging.BeginMessageScope(_logger, ea))
            using (var scope = _scopeFactory.CreateScope())
            {

                var status = scope.ServiceProvider.GetService<IJobStatusUpdater>();

                if (status is not null && meta is not null)
                {
                    await status.UpdateJobStatusAsync(meta.JobId, meta.BanyanLoadId, JobProcessingStatus.InProgress, $"Consuming from queue {_queueName}");
                }

                result = await HandleAsync(scope.ServiceProvider, msg, correlationId, CancellationToken.None);

                if (status is not null && meta is not null)
                {
                    if (result.IsSuccess)
                    {
                        await status.UpdateJobStatusAsync(meta.JobId, meta.BanyanLoadId, JobProcessingStatus.Success, $"Processed on queue {_queueName}");
                    }
                    else
                    {
                        await status.UpdateJobStatusAsync(meta.JobId, meta.BanyanLoadId, JobProcessingStatus.Failed, result.ErrorMessage ?? "Handler returned failure");
                    }
                }
            }

            if (_channel is null)
            {
                _logger.LogCritical("IChannel is null in RabbitWorkerBase");
                return;
            }

            if (result.IsSuccess)
            {
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            else
            {
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: result.Requeue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{Queue}] Failed message; sending to DLQ (nack requeue:false)", _queueName);

            if (_channel is not null)
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    /// <summary>
    /// Processes a message using application-specific logic. Must be implemented by derived classes.
    /// </summary>
    /// <param name="scopedProvider">The scoped <see cref="IServiceProvider"/> to resolve services for handling the message.</param>
    /// <param name="message">The deserialized message to process.</param>
    /// <param name="correlationId">The correlation ID associated with the message.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>
    /// A <see cref="MessageHandlerResult{TResult}"/> indicating the result of processing.
    /// </returns>
    protected abstract Task<MessageHandlerResult<TResult>> HandleAsync(IServiceProvider scopedProvider, TMessage message, Guid correlationId, CancellationToken ct);
}