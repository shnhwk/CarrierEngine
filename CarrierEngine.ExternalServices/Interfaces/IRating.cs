using System.Threading.Tasks;

namespace CarrierEngine.ExternalServices.Interfaces;

public interface IRating
{
    public Task RateLoad();
}