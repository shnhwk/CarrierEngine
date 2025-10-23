using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CarrierEngine.Data;
using CarrierEngine.Data.Models;
using CarrierEngine.ExternalServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CarrierEngine.ExternalServices;

public class CarrierCarrierConfigManagerManager : ICarrierConfigManager
{

    private Dictionary<string, CarrierTrackingCodeMap> _carrierTrackingCodeLookup;

    private readonly CarrierEngineDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CarrierCarrierConfigManagerManager> _logger;

    public CarrierCarrierConfigManagerManager(CarrierEngineDbContext dbContext, IDistributedCache cache, ILogger<CarrierCarrierConfigManagerManager> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> Set<T>(string key)
    {
        //TODO: cache
        var configString = await _dbContext.Carriers.Where(c=>c.CarrierKey == key).Select(c => c.ConfigurationData).FirstOrDefaultAsync();

        configString = Regex.Replace(configString, @"[^\u0000-\u007F]+", string.Empty);

        return JsonSerializer.Deserialize<T>(configString ?? throw new InvalidOperationException());
    }
 
    public async Task LoadMappingsAsync(string key)
    {
        _logger.LogTrace("Starting LoadMappingsAsync for key: {Key}", key);

        try
        {

            _logger.LogDebug("Attempting to retrieve data from cache for key: {Key}", key);
            var cachedData = await _cache.GetStringAsync(key);

            if (cachedData != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);

                _carrierTrackingCodeLookup = JsonSerializer.Deserialize<ICollection<CarrierTrackingCodeMap>>(cachedData)?
                    .ToDictionary(x => x.CarrierValue); 
                
                _logger.LogDebug("Deserialized cache data for key: {Key}", key);

                return;  
            }

            _logger.LogDebug("Cache miss for key: {Key}. Querying database.", key);

             var carrier = await _dbContext.Carriers
                .AsNoTracking()
                .Include(c => c.CarrierTrackingCodeMaps)
                .FirstOrDefaultAsync(c => c.CarrierKey == key); 

            if (carrier is null)
            {
                _logger.LogWarning("Carrier not found in the database for key: {Key}", key);
                throw new InvalidOperationException($"Carrier with key '{key}' not found.");
            }

            _carrierTrackingCodeLookup = carrier.CarrierTrackingCodeMaps.ToDictionary(x => x.CarrierValue);

            _logger.LogDebug("Loaded {Count} tracking codes for key: {Key}", _carrierTrackingCodeLookup.Count, key);

            var data = JsonSerializer.Serialize(carrier.CarrierTrackingCodeMaps);
            _logger.LogTrace("Serialized tracking codes for key: {Key}", key);

            await _cache.SetStringAsync(key, data, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            _logger.LogInformation("Data cached successfully for key: {Key} with expiration of 5 minutes.", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while loading mappings for key: {Key}", key);
            throw;  
        }
        finally
        {
            _logger.LogTrace("Finished LoadMappingsAsync for key: {Key}", key);
        }
    }

    public CarrierTrackingCodeMap GetTrackingCodeByCarrierValue(string carrierValue)
    {
        if (string.IsNullOrEmpty(carrierValue))
            throw new ArgumentNullException(nameof(carrierValue));

        if (_carrierTrackingCodeLookup == null || _carrierTrackingCodeLookup.Count == 0)
        {
            _logger.LogWarning("Tracking code lookup is empty. Did you forget to call LoadMappingsAsync?");
            throw new InvalidOperationException("Tracking code data is not loaded.");
        }

        if (_carrierTrackingCodeLookup.TryGetValue(carrierValue, out var result))
        {
            _logger.LogDebug("Found CarrierTrackingCodeMap for CarrierValue: {CarrierValue}", carrierValue);
            return result;
        }

        _logger.LogWarning("No CarrierTrackingCodeMap found for CarrierValue: {CarrierValue}", carrierValue);
        return null; // Or throw an exception if this is considered an error case
    }

}