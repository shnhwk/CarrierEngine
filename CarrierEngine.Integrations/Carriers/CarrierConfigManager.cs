using System;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Banyan.Cache;
using CarrierEngine.Domain.Interfaces;
using CarrierEngine.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.Integrations.Carriers;

public class CarrierConfigManager : ICarrierConfigManager
{
    private readonly CarrierEngineDbContext _dbContext;
    private readonly ICache _cache;
    private readonly ILogger<CarrierConfigManager> _logger;

    public CarrierConfigManager(CarrierEngineDbContext dbContext, ICache cache, ILogger<CarrierConfigManager> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> Set<T>(string key, CancellationToken ct)
    {
        
        if (string.IsNullOrWhiteSpace(key))
            return default;

        return await _cache.GetOrCreateAsync(key, async () =>
        {
            var configString = await _dbContext.Carriers
                .Where(c => c.CarrierKey == key)
                .Select(c => c.ConfigurationData)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(configString))
                return default;

            // Clean up non-ASCII characters if needed
            configString = Regex.Replace(configString, @"[^\u0000-\u007F]+", string.Empty);

            try
            {
                return JsonSerializer.Deserialize<T>(configString, JsonSerializerOptions.Web);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize configuration data for key: {Key}", key);
                return default;
            }
        }, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) });
    }
}