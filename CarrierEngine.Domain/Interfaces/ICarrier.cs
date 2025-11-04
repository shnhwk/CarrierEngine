namespace CarrierEngine.Domain.Interfaces
{
    public interface ICarrier
    {
        Task SubmitLogs(RequestResponseType requestResponseType, CancellationToken ct = default);

        Task SetCarrierConfig(string key, CancellationToken ct = default);

        void Initialize(CarrierDependencies dependencies, int banyanLoadId);

        Task SetTrackingMaps(string carrierKey, CancellationToken ct = default);
    }
}