using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CarrierEngine.ExternalServices;

public class HttpClientWrapper : IHttpClientWrapper
{
    private readonly HttpClient _httpClient;
    private readonly List<RequestResponseInfo> _logs = new();
    private TimeSpan? _customTimeout;
    private readonly CookieContainer _cookieContainer;

    private readonly Dictionary<string, string> _headers = new();
    private readonly Dictionary<string, string> _queryParameters = new();

    public HttpClientWrapper(IHttpClientFactory httpClientFactory)
    {
        _cookieContainer = new CookieContainer();

        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(100); // Default timeout
    }

    public IReadOnlyCollection<RequestResponseInfo> Logs => _logs.AsReadOnly();

    public IHttpClientWrapper WithTimeout(TimeSpan timeout)
    {
        _customTimeout = timeout;
        return this;
    }
    public IHttpClientWrapper WithTimeout(int timeoutSeconds)
    {
        _customTimeout = TimeSpan.FromSeconds(timeoutSeconds);
        return this;
    }

    public IHttpClientWrapper WithHeader(string name, string value)
    {
        if (!_headers.TryAdd(name, value))
        {
            _headers[name] = value;
        }

        return this;
    }

    public IHttpClientWrapper WithHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            WithHeader(header.Key, header.Value);
        }
        return this;
    }


    public IHttpClientWrapper WithContentType(string contentType)
    {
        WithHeader("Content-Type", contentType);
        return this;
    }

    public IHttpClientWrapper WithContentType(ContentType contentType)
    {
        var contentTypeString = contentType switch
        {
            ContentType.ApplicationJson => "application/json",
            ContentType.TextXml => "text/xml",
            ContentType.FormUrlEncoded => "application/x-www-form-urlencoded",
            ContentType.MultipartFormData => "multipart/form-data",
            _ => throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null)
        };

        return WithContentType(contentTypeString);
    }

    public IHttpClientWrapper WithBasicAuth(string username, string password)
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        return WithHeader("Authorization", $"Basic {credentials}");
    }

    public IHttpClientWrapper WithBearerToken(string token)
    {
        WithHeader("Bearer", token);
        return this;
    }

    public IHttpClientWrapper AddQueryParam(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Query parameter name cannot be null or empty", nameof(name));
        }

        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_\-]+$"))
        {
            throw new ArgumentException("Query parameter name contains invalid characters", nameof(name));
        }

        if (!_queryParameters.ContainsKey(name))
        {
            _queryParameters.Add(name, HttpUtility.UrlEncode(value));
        }
        else
        {
            _queryParameters[name] = HttpUtility.UrlEncode(value);
        }

        return this;

    }

    private string BuildQueryString()
    {
        if (_queryParameters.Count == 0)
            return string.Empty;

        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var param in _queryParameters)
        {
            query[param.Key] = param.Value;
        }

        return "?" + query;
    }

    private void ApplyHeaders(HttpRequestMessage request)
    {
        foreach (var header in _headers)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

    // Generic GET method with response deserialization
    public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveAsync<T>(HttpMethod.Get, url, null, cancellationToken);
    }

    // Generic POST method with request serialization and response deserialization
    public async Task<T> PostAsync<T>(string url, object content, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveAsync<T>(HttpMethod.Post, url, content, cancellationToken);
    }

    public async Task<TOut> PostJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveJsonStringAsync<TOut>(HttpMethod.Post, url, jsonContent, preserveHeaders, cancellationToken);
    }

    public async Task<TOut> PutJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveJsonStringAsync<TOut>(HttpMethod.Put, url, jsonContent, preserveHeaders, cancellationToken);
    }

    public async Task<TOut> PatchJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveJsonStringAsync<TOut>(HttpMethod.Patch, url, jsonContent, preserveHeaders, cancellationToken);
    }

    private async Task<TOut> SendAndReceiveJsonStringAsync<TOut>(HttpMethod method, string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default)
    {
        var queryString = BuildQueryString();
        var requestUrl = url + queryString;

        var request = new HttpRequestMessage(method, requestUrl);

        if (!string.IsNullOrWhiteSpace(jsonContent))
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        ApplyHeaders(request);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!preserveHeaders)
            _headers.Clear();

        return JsonSerializer.Deserialize<TOut>(responseBody) ?? throw new JsonException("Failed to deserialize response");
    }

    public async Task<T> PostFormUrlEncodedAsync<T>(string url, Dictionary<string, string> formData, CancellationToken cancellationToken = default)
    {
        var form = new FormUrlEncodedContent(formData);

        return await SendAndReceiveAsync<T>(HttpMethod.Post, url, form, cancellationToken);
    }

    // Generic PUT method with request serialization and response deserialization
    public async Task<T> PutAsync<T>(string url, object content, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveAsync<T>(HttpMethod.Put, url, content, cancellationToken);
    }

    // Generic PATCH method with request serialization and response deserialization
    public async Task<T> PatchAsync<T>(string url, object content, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveAsync<T>(HttpMethod.Patch, url, content, cancellationToken);
    }

    // Generic DELETE method with response deserialization
    public async Task<T> DeleteAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        return await SendAndReceiveAsync<T>(HttpMethod.Delete, url, null, cancellationToken);
    }

    // Private helper method to handle serialization, logging, and deserialization
    private async Task<T> SendAndReceiveAsync<T>(HttpMethod method, string url, object content = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, url);

        // Serialize request content to JSON if present
        if (content != null)
        {
            var jsonContent = JsonSerializer.Serialize(content);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        // If a custom timeout is set, create a CancellationTokenSource
        using var cts = _customTimeout.HasValue
            ? new CancellationTokenSource(_customTimeout.Value)
            : null;
        var linkedTokenSource = cts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token)
            : null;

        var effectiveCancellationToken = linkedTokenSource?.Token ?? cancellationToken;

        // Send the request and log the response
        var response = await SendAsync(request, effectiveCancellationToken);

        // Deserialize the response body to the specified type T
        var responseBody = await response.Content.ReadAsStringAsync(effectiveCancellationToken);
        return JsonSerializer.Deserialize<T>(responseBody) ?? throw new JsonException("Failed to deserialize response");
    }

    // Internal SendAsync method to log each request and response
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // Initialize the log entry
        var logEntry = new RequestResponseInfo
        {
            Type = "Request",
            //CorrelationId = _correlationId,
            Soap = false,
            Url = request.RequestUri?.ToString(),
            Method = request.Method.Method,
            RequestHeaders = JsonSerializer.Serialize(request.Headers),
            DateTime = DateTime.Now
        };

        try
        {
            // Attempt to read the request body and log it
            if (request.Content != null)
            {
                logEntry.RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            }
        }
        catch (Exception logException)
        {
            // Handle exceptions from reading the request body, log it if necessary
            // You can choose to log this exception somewhere else or handle it silently
            // For now, we'll just log it to a simple debug output
            Debug.WriteLine($"Error serializing request body: {logException.Message}");
        }

        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response = null;

        try
        {
            // Send the HTTP request and get the response
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception requestException)
        {
            // Handle the exception thrown by SendAsync (e.g., network failure, timeouts)
            // Log the request failure with status code 0 and capture the exception message
            logEntry.StatusCode = 0;
            logEntry.ResponseBody = $"Error: {requestException.Message}";

            // Log the failure, but don't interrupt the flow
            _logs.Add(logEntry);

            // Rethrow or return a default response if you want to propagate the error
            throw;
        }

        stopwatch.Stop();

        try
        {
            // Log the successful response
            logEntry.StatusCode = (int)response.StatusCode;
            logEntry.DurationMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            logEntry.ResponseHeaders = JsonSerializer.Serialize(response.Headers);

            logEntry.ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // Add the log entry even if reading the body fails
            _logs.Add(logEntry);
        }
        catch (Exception logException)
        {
            // Handle exceptions from logging response details (e.g., serialization errors)
            Debug.WriteLine($"Error logging response details: {logException.Message}");
        }

        // Return the response after logging
        return response;
    }

    // Helper method to set cookies
    public void SetCookie(Uri uri, string name, string value)
    {
        _cookieContainer.Add(uri, new Cookie(name, value));
    }

    // Helper method to retrieve cookies
    public CookieCollection GetCookies(Uri uri)
    {
        return _cookieContainer.GetCookies(uri);
    }
}