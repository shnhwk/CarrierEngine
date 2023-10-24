namespace CarrierEngine.ExternalServices.Interfaces;

public interface ICarrierFactory
{
    T GetCarrier<T>(string carrier);
}