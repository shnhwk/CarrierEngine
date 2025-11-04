namespace CarrierEngine.Domain.Dtos.Tracking;

public abstract class BaseResponseDto
{

    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }

}