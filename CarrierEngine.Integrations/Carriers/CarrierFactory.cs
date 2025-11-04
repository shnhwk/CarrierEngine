using System;
using System.Linq;
using System.Threading.Tasks;
using CarrierEngine.Domain;
using CarrierEngine.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CarrierEngine.Integrations.Carriers;

public class CarrierFactory : ICarrierFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CarrierFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<ICarrier> GetCarrier(string carrierName, int banyanLoadId)
    { 
        var type = typeof(CarrierFactory).Assembly.GetTypes().FirstOrDefault(t => t.Name == carrierName);

        if (type is null || !typeof(ICarrier).IsAssignableFrom(type))
            throw new ArgumentException($"Carrier '{carrierName}' not found or does not implement ICarrier.");
 
        if (_serviceProvider.GetService(type) is not ICarrier carrierInstance)
            throw new InvalidOperationException($"Service for carrier type '{type}' could not be retrieved.");

        var carrierDependencies = _serviceProvider.GetRequiredService<CarrierDependencies>();

        carrierInstance.Initialize(carrierDependencies, banyanLoadId);
         
        return Task.FromResult(carrierInstance);
    }
}