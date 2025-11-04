using System.Text;
using CarrierEngine.Infrastructure.Queues;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Infrastructure.Health;

/// <summary>
/// Publishes a test message using IRabbitQueuePublisher, then attempts to read it back
/// from the same queue to verify both publish and consume paths are working.
/// </summary>
public sealed class RabbitHealthCheck : IHealthCheck
{
    private readonly IRabbitQueuePublisher _publisher;
    private readonly IRabbitConnectionFactory _connectionFactory;
    private readonly ILogger<RabbitHealthCheck> _logger;

    public RabbitHealthCheck(
        IRabbitQueuePublisher publisher,
        IRabbitConnectionFactory connectionFactory,
        ILogger<RabbitHealthCheck> logger)
    {
        _publisher = publisher;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Use a dedicated queue for this check so we don't collide with app queues.
        const string queueName = "healthcheck.test.queue";
        var token = Guid.NewGuid().ToString("N"); // unique marker we look for on readback
        var correlationId = Guid.NewGuid();       // pass through your existing publisher path

        // Compose the tiny JSON body with the token we will match on read
        var payload = new { kind = "ping", token, utc = DateTimeOffset.UtcNow };
        try
        {
            // 1) Publish using your existing publisher (persisted, confirms enabled, etc.)
            await _publisher.SendToQueueAsync(
                message: payload,
                queueName: queueName,
                correlationId: correlationId,
                banyanLoadId: 0,               // not relevant for health
                ensureQueue: true,
                cancellationToken: cancellationToken);

            // 2) Open a channel and try to consume the message we just sent (poll briefly)
            await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var deadline = DateTime.UtcNow.AddSeconds(2); // small window is enough for local broker
            while (DateTime.UtcNow < deadline)
            {
                // NOTE: BasicGet is sync in RabbitMQ.Client; it's fine to call here.
                var result = await channel.BasicGetAsync(queue: queueName, autoAck: true, cancellationToken);
                if (result is not null)
                {
                    var body = Encoding.UTF8.GetString(result.Body.ToArray());

                    // Try to match our token (fast path: string.Contains to avoid allocations)
                    if (body.Contains(token, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogDebug("RabbitMQ round-trip health check succeeded via queue {Queue}.", queueName);
                        return HealthCheckResult.Healthy("Published and read back test message from RabbitMQ.");
                    }

                    // If the queue might carry other messages, just keep polling until deadline.
                }

                await Task.Delay(100, cancellationToken);
            }

            // If we published but never read our token back, treat as degraded/unhealthy
            return HealthCheckResult.Unhealthy("Published test message but did not read it back from RabbitMQ within the time window.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ round-trip health check failed.");
            return HealthCheckResult.Unhealthy($"RabbitMQ round-trip failed: {ex.Message}", ex);
        }
    }
}