namespace CarrierEngine.Domain.Results;

public record CarrierResult<T> : BaseResult<T>
{
    public static CarrierResult<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static CarrierResult<T> Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };

}