using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IRequestResponseLogger
{
    Task SubmitLogs(int banyanLoadId, IList<HttpRequestResponseLog> logs, RequestResponseType requestResponseType);
}