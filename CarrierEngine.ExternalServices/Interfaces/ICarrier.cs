using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface ICarrier
{
    Task SubmitLogs(RequestResponseType requestResponseType);

    Task SetCarrierConfig(string key);
}