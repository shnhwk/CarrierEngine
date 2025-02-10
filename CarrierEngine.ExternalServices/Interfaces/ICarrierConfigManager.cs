using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface ICarrierConfigManager
{
    Task<T> Set<T>(string key);
}