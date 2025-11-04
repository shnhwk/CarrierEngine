using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CarrierEngine.Domain.Dtos.Tracking;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Domain.Results;
using CarrierEngine.Infrastructure.Queues;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Consumer.TrackingRequests;

public sealed class TrackingConsumer : RabbitWorkerBase<TrackingJob, TrackingResponseDto>
{
    public TrackingConsumer(
        ILogger<TrackingConsumer> logger,
        IRabbitConnectionFactory connFactory,
        IServiceScopeFactory scopeFactory,
        string queueName)
        : base(logger, connFactory, scopeFactory, queueName, Deserialize) { }

    protected override Task<MessageHandlerResult<TrackingResponseDto>> HandleAsync(
        IServiceProvider sp, TrackingJob message, Guid correlationId, CancellationToken ct)
    {
        var handler = sp.GetRequiredService<ITrackingJobHandler>();  
        return handler.Handle(message, correlationId, ct);
    }

    private static (bool ok, TrackingJob? msg, string? reason) Deserialize(byte[] body)
    {
        var json = System.Text.Encoding.UTF8.GetString(body);
        try
        {
            var direct = JsonSerializer.Deserialize<TrackingJob>(json);
            if (direct is not null) return (true, direct, null);
        }
        catch { /* fall through */ }
        return (false, null, "not direct or envelope");
    }
}
