using CarrierEngine.Domain.Interfaces;

namespace CarrierEngine.Domain;

public sealed class CorrelationContext : ICorrelationContext
{
    private static readonly AsyncLocal<Guid> Current = new();
    public Guid Get() => Current.Value;
    public void Set(Guid value) => Current.Value = value;
}