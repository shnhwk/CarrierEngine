using System.Reflection;
using System.Threading.Tasks;
using CarrierEngine.ExternalServices.Carriers.ExampleCarrier;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IRating
{
    public Task RateLoad();
}