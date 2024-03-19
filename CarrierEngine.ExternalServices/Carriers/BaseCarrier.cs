using System.Threading.Tasks;
using CarrierEngine.ExternalServices.Interfaces;
using Flurl.Http;

namespace CarrierEngine.ExternalServices.Carriers;

public abstract class BaseCarrier
{
    protected int BanyanLoadId { get; set; }

    private readonly IRequestResponseLogger _requestResponseLogger;

    protected BaseCarrier(IRequestResponseLogger requestResponseLogger)
    {
        _requestResponseLogger = requestResponseLogger;
    }

    protected async Task LogRequest(FlurlCall flurlCall)
    {
        await _requestResponseLogger.Log(flurlCall);
    }

    protected async Task SubmitLogs(FluerlRequestResponseLogger.RequestResponseType requestResponseType)
    {
        await _requestResponseLogger.SubmitLogs(BanyanLoadId, requestResponseType);
    }

    public BaseCarrier For(int loadId)
    {
        BanyanLoadId = loadId;
        return this;
    }
}