using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices;

/// <summary>
/// The request response logger.
/// </summary>
public class FluerlRequestResponseLogger : IRequestResponseLogger
{
    private readonly ILogger<FluerlRequestResponseLogger> _logger;
    private List<HttpRequestResponseLog> Logs { get; } = new();

    public FluerlRequestResponseLogger(ILogger<FluerlRequestResponseLogger> logger)
    {
        _logger = logger;

    }

    public async Task Log(FlurlCall response)
    {
    }

    public async Task Log<T>(T r) 
    {
        var response = r as FlurlCall;

        var httpRequestMessage = response.HttpRequestMessage;
        var httpResponseMessage = response.HttpResponseMessage;

        _logger.LogInformation("Finished request in {ElapsedMilliseconds}ms", response.Duration?.TotalMilliseconds ?? 0);

        if (httpRequestMessage is null)
        {
            return;
        }

        if (httpResponseMessage is null)
        {
            return;

        }


        var scope = new Dictionary<string, object>();


        scope.TryAdd("response_code", httpResponseMessage.StatusCode);
        scope.TryAdd("request_headers", httpRequestMessage);

        var rConent = string.Empty;
        if (httpRequestMessage.Content != null)
        {
            rConent = await httpRequestMessage.Content.ReadAsStringAsync();
            scope.Add("request_body", rConent);
        }

        scope.TryAdd("response_headers", httpResponseMessage);

        var content = await httpResponseMessage.Content.ReadAsStringAsync();

        scope.Add("response_body", content);

        using (_logger.BeginScope(scope))
        {
            _logger.LogInformation("[TRACE] request/response");
        }


        Logs.Add(new HttpRequestResponseLog
        {
            RequestUrl = httpRequestMessage.RequestUri?.ToString(),
            RequestMethod = httpRequestMessage.Method.ToString(),
            RequestHeaders = httpRequestMessage.Headers.ToDictionary(h => h.Key, h => string.Join((string)",", (IEnumerable<string>)h.Value)),
            RequestTimestamp = DateTime.UtcNow,
            ResponseStatusCode = (int)httpResponseMessage.StatusCode,
            ResponseHeaders = httpResponseMessage.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
            RequestContent = rConent,
            ResponseContent = content,
            ResponseTimestamp = DateTime.UtcNow,
            DurationMilliseconds = response.Duration?.TotalMilliseconds ?? 0
        });
    }

    public async Task SubmitLogs()
    {
        try
        {
            await Task.Delay(100);
            _logger.LogInformation("{count} Logs submitted!", Logs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Request/Response Logs.");
        }
    }

}