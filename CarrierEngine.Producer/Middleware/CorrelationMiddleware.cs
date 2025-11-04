using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarrierEngine.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Producer.Middleware;

public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next; _logger = logger;
    }

    public async Task Invoke(HttpContext ctx, ICorrelationContext correlation)
    {
        var id = ctx.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue)
                 && Guid.TryParse(headerValue, out var parsed)
            ? parsed
            : Guid.NewGuid();

        correlation.Set(id);
        var requestId = Guid.NewGuid();


        ctx.Response.Headers["X-Correlation-ID"] = id.ToString();
        ctx.Response.Headers["X-Request-ID"] = requestId.ToString();

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id, ["RequestId"] = requestId }))
        {
            await _next(ctx);
        }
    }
}