using System.Net;

namespace CarrierEngine.Domain.Results;

public record ApiResult<T> : BaseResult<T>
{
    public HttpStatusCode StatusCode { get; init; }
    public string? RawBody { get; init; }

    public static ApiResult<T> Success(T data, HttpStatusCode code, string rawBody) => new() { IsSuccess = true, Data = data, StatusCode = code, RawBody = rawBody };

    public static ApiResult<T> Failure(HttpStatusCode code, string? body, string? message = null) => new() { IsSuccess = false, StatusCode = code, RawBody = body, ErrorMessage = message };
}