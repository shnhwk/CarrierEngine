using System.Diagnostics;
using CarrierEngine.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarrierEngine.Infrastructure.Health;

public class CarrierEngineDbHealthCheck : IHealthCheck
{
    private readonly CarrierEngineDbContext _dbContext;

    public CarrierEngineDbHealthCheck(CarrierEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            var stopwatch = Stopwatch.StartNew();
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken: cancellationToken);
            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds > 1000
                ? HealthCheckResult.Degraded($"Database is responding slowly ({stopwatch.ElapsedMilliseconds}ms).")
                : HealthCheckResult.Healthy("Database is operational.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unexpected database health check failure.", ex);
        }
    }
}