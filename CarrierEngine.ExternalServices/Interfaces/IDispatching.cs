using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IDispatching
{
    public Task DispatchLoad();
}