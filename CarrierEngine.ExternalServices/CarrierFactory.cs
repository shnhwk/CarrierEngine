using CarrierEngine.ExternalServices.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using CarrierEngine.Data;

namespace CarrierEngine.ExternalServices;

public class CarrierFactory : ICarrierFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CarrierFactory(IServiceProvider serviceProvider, CarrierEngineDbContext dbContext)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<ICarrier> GetCarrier(string carrierName)
    { 
        var type = typeof(CarrierFactory).Assembly.GetTypes().FirstOrDefault(t => t.Name == carrierName);

        if (type is null || !typeof(ICarrier).IsAssignableFrom(type))
            throw new ArgumentException($"Carrier '{carrierName}' not found or does not implement ICarrier.");
 
        if (_serviceProvider.GetService(type) is not ICarrier carrierInstance)
            throw new InvalidOperationException($"Service for carrier type '{type}' could not be retrieved.");
  
        //var setConfigTask = (Task)carrierInstance.GetType().GetMethod("SetCarrierConfig")?.Invoke(carrierInstance, null);
        //if (setConfigTask != null)
        //    await setConfigTask;
 

        return Task.FromResult(carrierInstance);
    }
}