namespace CarrierEngine.Domain.Results;

public abstract record BaseResult<T>
{
    public T? Data { get; init; }

    public bool IsSuccess { get; init; }

    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; init; }

}