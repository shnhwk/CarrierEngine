using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices;

public interface IHttpClientWrapper
{
    // Chained method to set a custom timeout for specific requests
    IHttpClientWrapper WithTimeout(TimeSpan timeout);
    IHttpClientWrapper WithTimeout(int timeoutSeconds);

    // Collection of request-response logs for the current session
    IReadOnlyCollection<RequestResponseInfo> Logs { get; }

    // HTTP operations with generic deserialization
    Task<T> GetAsync<T>(string url, CancellationToken cancellationToken = default);

    Task<T> PostAsync<T>(string url, object content, CancellationToken cancellationToken = default);


    Task<TOut> PostJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default);

    Task<TOut> PutJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default);

    Task<TOut> PatchJsonAsync<TOut>(string url, string jsonContent, bool preserveHeaders = false, CancellationToken cancellationToken = default);



    Task<T> PutAsync<T>(string url, object content, CancellationToken cancellationToken = default);
    Task<T> PatchAsync<T>(string url, object content, CancellationToken cancellationToken = default);
    Task<T> DeleteAsync<T>(string url, CancellationToken cancellationToken = default);

    // Methods to set and retrieve cookies
    void SetCookie(Uri uri, string name, string value);
    CookieCollection GetCookies(Uri uri);



    IHttpClientWrapper WithHeader(string name, string value);
    IHttpClientWrapper WithHeaders(Dictionary<string, string> headers);
    IHttpClientWrapper AddQueryParam(string name, string value);

    IHttpClientWrapper WithBasicAuth(string username, string password);
    IHttpClientWrapper WithBearerToken(string token);
    IHttpClientWrapper WithContentType(string contentType);
    IHttpClientWrapper WithContentType(ContentType contentType);
    Task SubmitLogs(int banyanLoadId, RequestResponseType requestResponseType);
}