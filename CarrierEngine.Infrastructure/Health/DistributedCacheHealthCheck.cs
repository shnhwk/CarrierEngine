using System.Diagnostics;
using Banyan.Cache;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CarrierEngine.Infrastructure.Health;

public class DistributedCacheHealthCheck : IHealthCheck
{
    private readonly ICache _cache;

    public DistributedCacheHealthCheck(ICache cache)
    {
        _cache = cache;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        const string healthCheckKey = "HealthCheckKey";
        const string healthCheckValue = "Health check test.";

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _cache.SetAsync(healthCheckKey, healthCheckValue);
            var get = await _cache.GetAsync<string>(healthCheckKey);

            stopwatch.Stop();

            if (!string.Equals(get, healthCheckValue, StringComparison.Ordinal))
                return HealthCheckResult.Unhealthy("Distributed cache read/write mismatch.");

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            return elapsedMs > 500
                ? HealthCheckResult.Degraded($"Cache healthy but slow ({elapsedMs} ms).")
                : HealthCheckResult.Healthy("Distributed cache is responsive.");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return HealthCheckResult.Unhealthy("Distributed cache threw an exception.", ex);
        }
    }

}