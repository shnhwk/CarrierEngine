using System.Threading.Tasks;
using static CarrierEngine.ExternalServices.FluerlRequestResponseLogger;

namespace CarrierEngine.ExternalServices.Interfaces;

/// <summary>
/// Interface IRequestResponseLogger
/// </summary>
public interface IRequestResponseLogger 
{
    public Task Log<T>(T response);
    public Task SubmitLogs(int banyanLoadId, RequestResponseType requestResponseType);
}