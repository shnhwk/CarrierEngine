using System;
using System.Threading.Tasks;
using MassTransit;
using Serilog;
using Serilog.Events;
using LogContext = Serilog.Context.LogContext;

namespace CarrierEngine.Consumer.TrackingRequests;

public class LoadIdLoggingMiddleware<T> : IFilter<ConsumeContext<T>> where T : class
{
    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {

        var consumeContext = context.GetPayload<ConsumeContext>();

        using (LogContext.PushProperty(nameof(consumeContext.CorrelationId), consumeContext.CorrelationId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty(nameof(consumeContext.MessageId), consumeContext.MessageId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty(nameof(consumeContext.RequestId), consumeContext.RequestId.GetValueOrDefault().ToString()))
        using (LogContext.PushProperty("BanyanLoadId", consumeContext.GetHeader<int>("BanyanLoadId", -1)))
        {
            try
            {
                await next.Send(context).ConfigureAwait(false);
            }
            catch (Exception ex) when (LogExceptionMessage(ex)) // This ensures we don't unwind the stack to log the exception
            {
                // This should never be entered
                throw;
            }
        }
    }

    private static bool LogExceptionMessage(Exception e)
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
            Log.Debug("{0}", e.ToString());
        return false;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("LoadIdLogging");
    }
}