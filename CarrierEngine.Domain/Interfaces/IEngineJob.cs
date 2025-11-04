namespace CarrierEngine.Domain.Interfaces;

public interface IEngineJob
{
    Guid JobId { get; }
    int? BanyanLoadId { get; } 
}