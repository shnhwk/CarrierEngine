namespace CarrierEngine.Domain.Interfaces;

public interface ICorrelationContext
{
    Guid Get();
    void Set(Guid value);
}