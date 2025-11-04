using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;

namespace CarrierEngine.Infrastructure.Queues;

/// <summary>
/// Provides helper methods for creating structured logging scopes around RabbitMQ message handling.
/// </summary>
public static class RabbitMessageLogging
{
    /// <summary>
    /// Begins a structured logging scope for a RabbitMQ message, capturing key metadata
    /// such as correlation ID, message ID, request ID, routing key, and delivery tag.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> used to begin the logging scope.</param>
    /// <param name="ea">The <see cref="BasicDeliverEventArgs"/> representing the received RabbitMQ message.</param>
    /// <returns>
    /// An <see cref="IDisposable"/> that ends the logging scope when disposed.  
    /// Returns a no-op <see cref="NullScope"/> if <see cref="ILogger.BeginScope{TState}(TState)"/> returns <c>null</c>.
    /// </returns>
    public static IDisposable BeginMessageScope(ILogger logger, BasicDeliverEventArgs ea)
    {
        var props = ea.BasicProperties;
        var headers = props.Headers;

        var correlationId = props.CorrelationId;
        var messageId = props.MessageId;
        var requestId = HeaderAsString(headers, "RequestId");
        var banyanLoadId = HeaderAsInt(headers, "BanyanLoadId");
        var routingKey = ea.RoutingKey;
        var deliveryTag = ea.DeliveryTag;

        // This scope will automatically flow into Serilog because of UseSerilog()
        return logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = correlationId,
            ["MessageId"] = messageId,
            ["RequestId"] = requestId,
            ["BanyanLoadId"] = banyanLoadId,
            ["RoutingKey"] = routingKey,
            ["DeliveryTag"] = deliveryTag
        })?? new NullScope();
    }

    /// <summary>
    /// Retrieves a string value from message headers, converting common RabbitMQ header value types if needed.
    /// </summary>
    /// <param name="headers">The message header dictionary, or <c>null</c> if unavailable.</param>
    /// <param name="key">The key of the header to retrieve.</param>
    /// <returns>
    /// The decoded string value of the header, or <c>null</c> if the header is not found or cannot be converted.
    /// </returns>
    private static string? HeaderAsString(IDictionary<string, object?>? headers, string key)
    {
        if (headers is null || !headers.TryGetValue(key, out var v) || v is null)
            return null;

        return v switch
        {
            byte[] b => Encoding.UTF8.GetString(b),
            ReadOnlyMemory<byte> m => Encoding.UTF8.GetString(m.ToArray()),
            string s => s,
            _ => v.ToString()
        };
    }

    /// <summary>
    /// Retrieves an integer value from message headers, converting from string if possible.
    /// </summary>
    /// <param name="headers">The message header dictionary, or <c>null</c> if unavailable.</param>
    /// <param name="key">The key of the header to retrieve.</param>
    /// <returns>
    /// The integer value of the header if parsing succeeds, otherwise <c>null</c>.
    /// </returns>
    private static int? HeaderAsInt(IDictionary<string, object?>? headers, string key)
    {
        var s = HeaderAsString(headers, key);
        return int.TryParse(s, out var i) ? i : null;
    }

    /// <summary>
    /// Represents a no-operation disposable scope used when <see cref="ILogger.BeginScope{TState}(TState)"/> returns <c>null</c>.
    /// </summary>
    /// <remarks>This was an AI suggestion since BeginScope can return null.  I may revisit, but it seems fine for now. </remarks>
    private sealed class NullScope : IDisposable { public void Dispose() { } }

}