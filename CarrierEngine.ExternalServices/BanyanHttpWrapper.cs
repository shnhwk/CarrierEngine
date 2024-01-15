using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices
{

    public class BanyanHttpWrapper
    {
        private readonly HttpClient _httpClient;
        private readonly RequestLogger _requestLogger;

        public BanyanHttpWrapper(IHttpClientFactory httpClientFactory, RequestLogger requestLogger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _requestLogger = requestLogger;
        }


        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            _requestLogger.LogRequest(requestUri, "GET");
            var response = await _httpClient.GetAsync(requestUri);
            _requestLogger.LogResponse(requestUri, response.StatusCode, await response.Content.ReadAsStringAsync());
            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            _requestLogger.LogRequest(requestUri, "POST");
            var response = await _httpClient.PostAsync(requestUri, content);
            _requestLogger.LogResponse(requestUri, response.StatusCode, await response.Content.ReadAsStringAsync());
            return response;
        }


        // ... rest of the class remains the same
    }
}


public class RequestLogger
{
    private readonly ILogger<RequestLogger> _logger;
    private readonly Stopwatch _stopwatch = new Stopwatch();

    public RequestLogger(ILogger<RequestLogger> logger)
    {
        _logger = logger;
    }

    public void LogRequest(string url, string method)
    {
        _stopwatch.Start();
        _logger.LogInformation("Sending request: {Method} {Url}", method, url);
    }

    public void LogResponse(string url, HttpStatusCode statusCode, string responseBody = null)
    {
        _stopwatch.Stop();
        TimeSpan elapsed = _stopwatch.Elapsed;
        _logger.LogInformation("Received response: {StatusCode} in {ElapsedMilliseconds}ms", statusCode, elapsed.TotalMilliseconds);
        if (!string.IsNullOrEmpty(responseBody))
        {
            _logger.LogInformation("Response body: {ResponseBody}", responseBody);
        }
    }
}