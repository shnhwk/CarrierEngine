namespace CarrierEngine.Domain.Interfaces
{
    public interface ICarrierFactory
    {
        Task<ICarrier> GetCarrier(string carrierName, int banyanLoadId);
    }
}