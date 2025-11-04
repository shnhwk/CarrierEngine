namespace CarrierEngine.Domain.Interfaces;

public interface ICarrierConfigManager
{
    Task<T?> Set<T>(string key, CancellationToken ct);
}