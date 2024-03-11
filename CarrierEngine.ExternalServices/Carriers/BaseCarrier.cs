using System.Threading.Tasks;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;

namespace CarrierEngine.ExternalServices.Carriers;

public abstract class BaseCarrier
{
    private readonly IRequestResponseLogger _requestResponseLogger;

    protected BaseCarrier(IRequestResponseLogger requestResponseLogger)
    {
        _requestResponseLogger = requestResponseLogger;
    }

    protected async Task LogRequest(FlurlCall flurlCall)
    {
        await _requestResponseLogger.Log(flurlCall);
    }

    protected async Task SubmitLogs()
    {
        await _requestResponseLogger.SubmitLogs();
    }

}