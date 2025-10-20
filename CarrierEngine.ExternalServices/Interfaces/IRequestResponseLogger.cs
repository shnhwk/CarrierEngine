using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IRequestResponseLogger
{
    Task SubmitLogs(int banyanLoadId, IReadOnlyCollection<RequestResponseInfo> logs, RequestResponseType requestResponseType);
}