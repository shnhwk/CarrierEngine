using CarrierEngine.ExternalServices.Interfaces;
using System;
using System.Linq;

namespace CarrierEngine.ExternalServices;

public class CarrierFactory : ICarrierFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CarrierFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }


    public T GetCarrier<T>(string carrier)
    {
        var type = typeof(CarrierFactory).Assembly.GetTypes().FirstOrDefault(t => t.Name == carrier);

        if (type == null)
        {
            return default;
        }

        return (T)_serviceProvider.GetService(type);
    }
}