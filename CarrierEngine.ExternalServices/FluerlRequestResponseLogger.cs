using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices;

/// <summary>
/// The request response logger.
/// </summary>
public class FluerlRequestResponseLogger : IRequestResponseLogger
{
    private readonly ILogger<FluerlRequestResponseLogger> _logger;
    private readonly CarrierEngineDbContext _dbContext;
    private List<HttpRequestResponseLog> RequestLogs { get; } = new();

    public FluerlRequestResponseLogger(ILogger<FluerlRequestResponseLogger> logger, CarrierEngineDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Log<T>(T r)
    {
        var response = r as FlurlCall;

        var httpRequestMessage = response.HttpRequestMessage;
        var httpResponseMessage = response.HttpResponseMessage;

        _logger.LogInformation("Finished request in {ElapsedMilliseconds}ms", response.Duration?.TotalMilliseconds ?? 0);

        if (httpRequestMessage is null)
        {
            _logger.LogInformation("Request is null");
            return;
        }

        if (httpResponseMessage is null)
        {
            _logger.LogInformation("Response is null");
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

        var count = RequestLogs.Count(rl=>rl.Type == "Request");
        var requestLog = new HttpRequestResponseLog
        {
            Type = "Request",
            Number = count
        };
         
        requestLog.RequestResponseInfo.Add(new RequestResponseInfo
            {
                Type = "Request",
                Url = httpRequestMessage.RequestUri?.ToString(),
                Soap = false,
                Method = httpRequestMessage.Method.ToString(),
                ContentType = GetContentType(httpRequestMessage.Headers),
                Headers = GetHeaderString(httpRequestMessage.Headers),
                DateTime = DateTime.Now.ToString("G"),
                StatusCode = httpResponseMessage.StatusCode.ToString(),
                Data = rConent,
                DurationMilliseconds = response.Duration?.TotalMilliseconds ?? 0
            }
        );

        RequestLogs.Add(requestLog);


        var responseLog = new HttpRequestResponseLog
        {
            Type = "Response",
            Number = count
        };

        responseLog.RequestResponseInfo.Add(new RequestResponseInfo
            {
                Type = "Response",
                Url = httpRequestMessage.RequestUri?.ToString(),
                Soap = false,
                Method = httpRequestMessage.Method.ToString(),
                ContentType = GetContentType(httpResponseMessage.Headers),
                Headers = GetHeaderString(httpResponseMessage.Headers),
                DateTime = DateTime.Now.ToString("G"),
                StatusCode = httpResponseMessage.StatusCode.ToString(),
                Data = rConent,
                DurationMilliseconds = response.Duration?.TotalMilliseconds ?? 0
            }
        );

        RequestLogs.Add(responseLog);

    }
 

    private static string GetContentType(HttpHeaders headers)
    {
        return headers.TryGetValues("Content-Type", out var values)
            ? values.GetEnumerator().Current
            : string.Empty;
    }

    private static string GetHeaderString(HttpHeaders headers)
    {
        {
            var stringBuilder = new StringBuilder();

            foreach (var header in headers)
            {
                stringBuilder.Append($"{header.Key}:{string.Join(",", header.Value)}|");
            }

            return stringBuilder.ToString().TrimEnd('|');
        }
    }

    public async Task SubmitLogs(int banyanLoadId, RequestResponseType requestResponseType)
    {
        try
        {
            //simulate DB action
            await Task.Delay(100);

            foreach (var requestLog in RequestLogs)
            {
                _logger.LogInformation("Logging request/response {type} for {BanyanLoadId} number {number}", requestLog.Type, banyanLoadId, requestLog.Number);
            }

            _logger.LogInformation("{count} Logs submitted!", RequestLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Request/Response Logs.");
        }
    }

    public enum DataType
    {
        Json = 1,
        Xml = 2, 
        Text = 3,
    }


    public enum RequestResponseType
    {
        RateRequest = 6,
        RateResponse = 7,

        PickupRequest = 8,
        PickupResponse = 9,

        TrackingRequest = 10,
        TrackingResponse = 11,

        FailedRateRequest = 18,
        FailedRateResponse = 19,



        PostLoadRequest = 35,
        PostLoadResponse = 36,
        CancelPostLoadRequest = 37,
        CancelPostLoadResponse = 38,
        NegotiationRequest = 39,
        NegotiationResponse = 40,
        ConfirmationRequest = 41,
        ConfirmationResponse = 42,
        LoadTrackRequest = 43,
        LoadTrackResponse = 44
    }
}