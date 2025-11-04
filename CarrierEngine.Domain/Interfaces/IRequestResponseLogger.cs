namespace CarrierEngine.Domain.Interfaces;

public interface IRequestResponseLogger
{
    Task SubmitLogs(int banyanLoadId, IReadOnlyCollection<RequestResponseInfo> logs, RequestResponseType requestResponseType);
}